using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class VolunteerShiftController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerShiftController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: VolunteerShift
        public async Task<IActionResult> Index(int? VolunteerID, int? page, int? pageSizeID, string actionButton,
            string SearchString)
        {
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

            return View(shifts);
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
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
        }

        // GET: VolunteerShift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
        }

        // POST: VolunteerShift/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EventID,ShiftStart,ShiftEnd")] Shift shift)
        {
            if (id != shift.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shift);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shift.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
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

        private SelectList EventSelectList(int? selectedId)
        {
            return new SelectList(_context.Events
                .OrderBy(e => e.EventName), "ID", "EventName", selectedId); 
        }
        
        private void PopulateDropDownList(Shift? shift = null)
        {
            ViewData["Events"] = EventSelectList(shift?.EventID);
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
