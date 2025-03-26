using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Data.Migrations;
using TVAttendance.Models;
using TVAttendance.Utilities;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class VolunteerShiftController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerShiftController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: VolunteerShift
        [Authorize]
        public async Task<IActionResult> Index(int? VolunteerID, int? page, string actionButton, DateTime? toDate, DateTime? fromDate,
            string SearchEventName, bool ActiveStatus = true)
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");

            if (!VolunteerID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            var shifts = _context.Shifts
                        .Include(a => a.Event)
                        .Include(a => a.ShiftVolunteers)
                        .ThenInclude(a => a.Volunteer)
                        .Where(a => a.ShiftVolunteers.Select(v => v.VolunteerID).FirstOrDefault() == VolunteerID)
                        .OrderBy(a=>a.ShiftStart)
                        .AsNoTracking();

            Volunteer? volunteer = await _context.Volunteers
                .Include(p => p.ShiftVolunteers)
                .ThenInclude(s=>s.Shift)
                .ThenInclude(e=>e.Event)
                .Where(p => p.ID == VolunteerID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            IQueryable<Event> events = _context.Events
                .Include(s=>s.Shifts)
                .ThenInclude(v=>v.ShiftVolunteers)
                .Where(e=>e.Shifts.Any(v=>v.ShiftVolunteers.Any(v=>v.VolunteerID == VolunteerID)))
                .AsNoTracking();

            events = EventFilter(events, SearchEventName, toDate, fromDate);
            if (fromDate == null && toDate == null)
            {
                events = events.Where(s => s.EventStart.CompareTo(DateTime.Now) >= 0);
            }
            if (!SearchEventName.IsNullOrEmpty())
            {
                numFilters++;
            }
            if (toDate.HasValue)
            {
                numFilters++;
            }
            if (fromDate.HasValue)
            {
                numFilters++;
            }


            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }

            int pageSize = 3;
            ViewBag.Volunteer = volunteer;
            ViewBag.Events = await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), page ?? 1, pageSize);
            ViewData["Upcoming"] = ActiveStatus;

            return View(shifts);
        }

        public IQueryable<Event> EventFilter(IQueryable<Event> events, string? EventName, DateTime? toDate, DateTime? fromDate)
        {
            if (!EventName.IsNullOrEmpty())
            {
                events = events.Where(e => e.EventName.ToUpper().Contains(EventName.ToUpper())).OrderBy(e=>e.EventName);
            }
            if (toDate.HasValue)
            {
                events = events.Where(s => s.EventStart <= toDate);
            }
            if (fromDate.HasValue)
            {
                events = events.Where(s => s.EventStart >= fromDate);
            }

            return events.OrderBy(e=>e.EventStart);
        }
        [Authorize]
        public async Task<IActionResult> ClockIn(int id, int VolunteerID)
        {
            var shift = await _context.ShiftVolunteers
                .Include(s => s.Volunteer)
                .Where(s=>s.VolunteerID == VolunteerID)
                .FirstOrDefaultAsync(s => s.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shift.ClockIn = DateTime.Now;
                _context.Update(shift);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] =  $"{shift.Volunteer.FullName} has clock-in at {shift.ClockIn.Value.ToShortTimeString()}";
                var returnURL = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnURL))
                {
                    return RedirectToAction(nameof(Index), new { VolunteerID = VolunteerID });
                }
                return RedirectToAction(returnURL);
            }
            else
            {
                return View(shift);
            }
        }
        [Authorize]
        public async Task<IActionResult> ClockOut(int id, int VolunteerID)
        {
            var shift = await _context.ShiftVolunteers
                .Include(s => s.Volunteer)
                .Where(s => s.VolunteerID == VolunteerID)
                .FirstOrDefaultAsync(s => s.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shift.ClockOut = DateTime.Now;
                _context.Update(shift);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = $"{shift.Volunteer.FullName} has clock-in at {shift.ClockOut.Value.ToShortTimeString()}";
                var returnURL = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnURL))
                {
                    return RedirectToAction(nameof(Index), new {VolunteerID = VolunteerID});
                }
                return RedirectToAction(returnURL);
            }

            return View(shift);
        }
        [Authorize]
        public async Task<IActionResult> AttendanceConfirm(int? id)
        {
            var shift = await _context.ShiftVolunteers
                .Include(s => s.Shift)
                .FirstOrDefaultAsync(sv=>sv.ShiftID == id);

            if(shift == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int volId = shift.VolunteerID;
                shift.NonAttendance = true;
                _context.Update(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = volId });
            }

            return View(shift);
        }
        [Authorize]
        public async Task<IActionResult> AttendanceDeny(int? id)
        {
            var shift = await _context.ShiftVolunteers
                .Include(s => s.Shift)
                .FirstOrDefaultAsync(sv => sv.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int volId = shift.VolunteerID;
                shift.NonAttendance = false;
                _context.Update(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = volId });
            }

            return View(shift);
        }

        // GET: VolunteerShift/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }

            

            return View(shift);
        }

        // GET: VolunteerShift/Create
        [Authorize]
        public async Task<IActionResult> Take(int? EventID, int VolunteerID)
        {
            ViewData["ModalPopupShift"] = "hide";

            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == EventID);

            Volunteer? thisVolunteer = await _context.Volunteers
                .AsNoTracking()
                .FirstOrDefaultAsync(v=>v.ID == VolunteerID);

            DateTime date = thisEvent.EventStart.Date;

            //ViewData's for display only
            ViewData["EventName"] = thisEvent.EventName;
            ViewData["EventStart"] = thisEvent.EventStart;
            ViewData["EventEnd"] = thisEvent.EventEnd;
            ViewData["EventRange"] = thisEvent.EventDate;

            ViewBag.Volunteer = thisVolunteer;
            ViewBag.Event = thisEvent;

            //Create a new empty shift with an event id
            Shift shift = new Shift
            {
                EventID = thisEvent.ID
            };
            //new empty shift with an event id
            return View(shift);
        }

        // POST: VolunteerShift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Take([Bind("ShiftStart,ShiftEnd")] Shift shift, int VolunteerID, int EventID)
        {
            if (ModelState.IsValid)
            {
                shift.EventID = EventID;

                _context.Add(shift);
                await _context.SaveChangesAsync();

                ShiftVolunteer volShift = new ShiftVolunteer
                {
                    ShiftID = shift.ID,
                    VolunteerID = VolunteerID
                };

                _context.Add(volShift);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = VolunteerID});
            }
            return View(shift);
        }

        // GET: VolunteerShift/Edit/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ShiftVolunteers == null)
            {
                return NotFound();
            }

            var shift = await _context.ShiftVolunteers
                .Include(e=>e.Shift)
                .ThenInclude(e => e.Event)
                .Include(v=>v.Volunteer)
                .AsNoTracking()
                .FirstOrDefaultAsync(s=>s.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // POST: VolunteerShift/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var shiftToUpdate = await _context.Shifts
                .Include(e => e.Event)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (shiftToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Shift>(shiftToUpdate, "", s => s.ShiftStart, s => s.ShiftEnd))
            {
                try
                {
                    _context.Update(shiftToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shiftToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(shiftToUpdate);
        }
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var shiftToUpdate = await _context.ShiftVolunteers
                .FirstOrDefaultAsync(s => s.ShiftID == id);

            if (shiftToUpdate == null)
            {
                return NotFound();
            }

            shiftToUpdate.NonAttendance = false;

            if (await TryUpdateModelAsync<ShiftVolunteer>(shiftToUpdate, "", s => s.NonAttendance, s=>s.AttendanceReason, s=>s.Note))
            {
                try
                {
                    _context.Update(shiftToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Shift cancelled successfully";
                    return RedirectToAction("Index", "VolunteerShift", new {VolunteerID = shiftToUpdate.VolunteerID});
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shiftToUpdate.ShiftID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            TempData["ErrorMsg"] = "Could not cancel the shift.  Contact an Administrator";
            return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = shiftToUpdate.VolunteerID });
        }

        // GET: VolunteerShift/Delete/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // POST: VolunteerShift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //private SelectList EventSelectList(int? selectedId)
        //{
        //    return new SelectList(_context.Events
        //        .OrderBy(e => e.EventName), "ID", "EventName", selectedId); 
        //}
        
        //private void PopulateDropDownList(Shift? shift = null)
        //{
        //    ViewData["Events"] = EventSelectList(shift?.EventID);
        //}

        //public async void MissedShift(IQueryable<Shift> shifts)
        //{
        //    shifts = shifts.Where(s => s.ShiftDate.ToDateTime(TimeOnly.MinValue).Date < DateTime.Now.Date &&
        //        s.ShiftVolunteers.Any(s=>s.ClockIn == null));

        //    List<Shift> shiftList = shifts.ToList();

        //    if(shifts != null)
        //    {
        //        int numOfShifts = 0;

        //        foreach(Shift shift in shiftList)
        //        {
        //            if (await TryUpdateModelAsync<Shift>(shifts.Where(s=>s.ID == shift.ID).FirstOrDefault(), "", s=>s.ShiftVolunteers.Where(s=>s.ShiftID == shift.ID).Select(s=>s.NonAttendance)))
        //            {
        //                try
        //                {
        //                    _context.Update(shift);
        //                    await _context.SaveChangesAsync();
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!ShiftExists(shift.ID))
        //                    {
                                
        //                    }
        //                    else
        //                    {
        //                        throw;
        //                    }
        //                }
        //            }

        //            numOfShifts++;
        //        }

        //        TempData["ErrorMsg"] = $"You missed {numOfShifts} shift(s)";
        //    }
        //}

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }

        public async Task<IActionResult> IdUpdate(int ShiftID)
        {
            var selectedShift = await _context.ShiftVolunteers
                .Include(v => v.Volunteer)
                .Include(s => s.Shift)
                .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(s => s.ShiftID == ShiftID);

            return PartialView("_VolShiftDetails", selectedShift);
        }
    }
}
