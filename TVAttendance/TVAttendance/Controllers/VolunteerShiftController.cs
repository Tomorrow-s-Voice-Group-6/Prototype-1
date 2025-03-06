using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

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
        public async Task<IActionResult> Index(int? VolunteerID, int? page, string actionButton,
            string SearchString, bool ActiveStatus = true)
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");

            if (!VolunteerID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            var shifts = from a in _context.Shifts
                        .Include(a => a.Event)
                        .Include(a => a.ShiftVolunteers)
                        .ThenInclude(a => a.Volunteer)
                        where a.ShiftVolunteers.Select(v=>v.VolunteerID).FirstOrDefault() == VolunteerID
                        select a;

            shifts = ActiveStatus ? shifts.Where(s => s.ShiftDate > DateOnly.FromDateTime(DateTime.Now)) : shifts.Where(s => s.ShiftDate < DateOnly.FromDateTime(DateTime.Now)).OrderBy(s=>s.ShiftDate);

            if (!SearchString.IsNullOrEmpty())
            {
                shifts = shifts.Where(s => s.Event.EventName.Contains(SearchString)).OrderBy(s=>s.Event.EventName);
                numFilters++;
            }
            if (!ActiveStatus)
            {
                numFilters++;
            }
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }


            Volunteer? volunteer = await _context.Volunteers
                .Include(p => p.ShiftVolunteers)
                .ThenInclude(s=>s.Shift)
                .ThenInclude(e=>e.Event)
                .Include(p => p.ShiftVolunteers)
                .ThenInclude(s => s.Shift)
                .Where(p => p.ID == VolunteerID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Volunteer = volunteer;

            ViewData["Upcoming"] = ActiveStatus;
            int pageSize = 3;
            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: VolunteerShift/Details/5
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
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity");
            return View();
        }

        // POST: VolunteerShift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EventID,ShiftStart,ShiftEnd")] Shift shift)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shift);
        }

        // GET: VolunteerShift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Shifts == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(e=>e.Event)
                .AsNoTracking()
                .FirstOrDefaultAsync(s=>s.ID==id);

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
        public async Task<IActionResult> Edit(int id)
        {
            var shiftToUpdate = await _context.Shifts
                .Include(e => e.Event)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (shiftToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Shift>(shiftToUpdate, "", s => s.ShiftDate, s => s.ShiftStart, s => s.ShiftEnd))
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

        // GET: VolunteerShift/Delete/5
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

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
