using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TVAttendance.Data;
using TVAttendance.Data.Migrations;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class EventShiftController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public EventShiftController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: EventShift
        public async Task<IActionResult> Index(int? EventID, int? page=1)
        {
            if (EventID == null)
            {
                EventID = ViewBag.EventID;
            }
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

            var shifts = _context.Shifts.Include(s => s.Event)
                .Where(e => e.EventID == EventID)
                .AsNoTracking();

            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == EventID);

            ViewBag.Event = thisEvent;
            ViewBag.EventID = EventID;

            int pageSize = 3;
            int pageIndex = page ?? 1;
            int totalItems = await shifts.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts, pageIndex, pageSize);

            // Pass pagination details to the view
            ViewData["CurrentPage"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View(pagedData);
        }

        // GET: EventShift/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);

            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == shift.EventID);

            ViewBag.EventID = id;
            ViewBag.Event = thisEvent;

            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // GET: EventShift/Create
        public async Task<IActionResult> Create(int? id)
        {
            
            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == id);

            ViewBag.EventID = id;
            ViewBag.Event = thisEvent;

            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", id);
            return View();
        }

        // POST: EventShift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EventID,ShiftStart,ShiftEnd")] Shift shift)
        {
            try
            {
                Event? thisEvent = await _context.Events
                    .Include(s => s.Shifts)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ID == shift.EventID);
                shift.Event = thisEvent;
                ViewBag.EventID = shift.EventID;
                ViewBag.Event = thisEvent;
                if (ModelState.IsValid)
                {
                    _context.Add(shift);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {

            }
            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
        }

        // GET: EventShift/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(e => e.Event)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (shift == null)
            {
                return NotFound();
            }
            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == shift.EventID);

            ViewBag.EventID = shift.EventID;
            ViewBag.Event = thisEvent;

            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
        }

        // POST: EventShift/Edit/5
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

        // GET: EventShift/Delete/5
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

        // POST: EventShift/Delete/5
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

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
