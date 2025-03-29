using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        public async Task<IActionResult> Index(int? VolunteerID, int? page, int? pageSizeID, string actionButton, DateTime? toDate, DateTime? fromDate,
            string SearchEventName, bool? Attendance = null)
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");

            if (!VolunteerID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            Volunteer? volunteer = await _context.Volunteers
                .Include(p => p.ShiftVolunteers)
                .ThenInclude(s => s.Shift)
                .ThenInclude(e => e.Event)
                .Where(p => p.ID == VolunteerID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var shifts = _context.ShiftVolunteers
                        .Include(a => a.Shift)
                        .ThenInclude(a => a.Event)
                        .Include(a => a.Volunteer)
                        .Where(v => v.VolunteerID == VolunteerID)
                        .AsNoTracking();

            if (Attendance.HasValue)
            {
                shifts = shifts.Where(e => e.NonAttendance.Value).OrderByDescending(e => e.Shift.ShiftStart);
                numFilters++;
            }
            else
            {
                //need to populate
            }
            if (!SearchEventName.IsNullOrEmpty())
            {
                shifts = shifts.Where(e => e.Shift.Event.EventName.ToUpper().Contains(SearchEventName.ToUpper())).OrderByDescending(e => e.Shift.ShiftStart);
                numFilters++;
            }
            if (toDate.HasValue)
            {
                shifts = shifts.Where(s => s.Shift.ShiftStart <= toDate).OrderByDescending(e => e.Shift.ShiftStart);
                numFilters++;
            }
            if (fromDate.HasValue)
            {
                shifts = shifts.Where(s => s.Shift.ShiftEnd >= fromDate).OrderByDescending(e => e.Shift.ShiftStart);
                numFilters++;
            }
            if (fromDate == null && toDate == null)
            {
                shifts = shifts.Where(s => s.Shift.ShiftStart.CompareTo(DateTime.Now) >= 0);
            }


            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }

            ViewBag.Volunteer = volunteer;
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<ShiftVolunteer>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
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
                int? volId = shift.VolunteerID;
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
                int? volId = shift.VolunteerID;
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

            var shift = await _context.ShiftVolunteers
                .Include(s => s.Shift)
                .ThenInclude(e=>e.Event)
                .FirstOrDefaultAsync(m => m.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_VolShiftDetails", shift);
            }

            return View(shift);
        }

        // GET: VolunteerShift/Create
        [Authorize]
        public async Task<IActionResult> Take(int? id)
        {
            ViewData["ModalPopupShift"] = "hide";

            var shift = _context.Shifts
                .Include(e=>e.Event)
                .FirstOrDefault(s=>s.ID == id);

            var shiftTaker = _context.Volunteers
                .FirstOrDefault(v => v.Email == User.Identity.Name);

            if(shiftTaker == null || shift == null)
            {
                return NotFound();
            }

            ShiftVolunteer takenShift = new ShiftVolunteer
            {
                ShiftID = shift.ID,
                VolunteerID = shiftTaker.ID
            };

            try
            {
                _context.Add(takenShift);
                _context.SaveChanges();

                TempData["SuccessMsg"] = "Shift Taken";
                return RedirectToAction("Index", "EventShift", new { EventID = shift.EventID });
            }
            catch
            {

            }
            
            //new empty shift with an event id
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
            var shiftVolToUpdate = await _context.ShiftVolunteers
                .Include(e => e.Shift)
                .Include(v=>v.Volunteer)
                .FirstOrDefaultAsync(s => s.ShiftID == id);
            var shiftToUpdate = await _context.Shifts
                .Include(e => e.Event)
                .FirstOrDefaultAsync(s => s.ID == id);


            if (shiftVolToUpdate == null || shiftToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Shift>(shiftToUpdate, "", s=>s.ShiftStart, s=>s.ShiftEnd))
            {
                try
                {
                    _context.Update(shiftToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shiftVolToUpdate.ShiftID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (await TryUpdateModelAsync<ShiftVolunteer>(shiftVolToUpdate, "", s => s.ClockIn, s => s.ClockOut, s=>s.NonAttendance, s=>s.AttendanceReason))
            {
                try
                {
                    _context.Update(shiftVolToUpdate);
                    await _context.SaveChangesAsync();

                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shiftVolToUpdate.ShiftID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            
            return View(shiftVolToUpdate);
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


        // GET
        [Authorize]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.ShiftVolunteers
                .Include(s => s.Shift)
                .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(m => m.ShiftID == id);

            if (shift == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Cancel", shift);
            }

            shift.NonAttendance = false;

            if (await TryUpdateModelAsync<ShiftVolunteer>(shift, "", s => s.NonAttendance, s => s.AttendanceReason))
            {
                try
                {
                    _context.Update(shift);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Shift cancelled successfully";
                    return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = shift.VolunteerID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shift.ShiftID))
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
            return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = shift.VolunteerID });
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
