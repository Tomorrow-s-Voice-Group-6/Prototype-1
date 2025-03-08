using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TVAttendance.Data;
using TVAttendance.Data.Migrations;
using TVAttendance.Models;
using TVAttendance.Utilities;
using TVAttendance.ViewModels;

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
        public async Task<IActionResult> Index(string? actionButton, DateOnly? fromDate, DateOnly? toDate, int? EventID, int? page = 1,
            string sortDirection = "asc", string sortField = "Location")
        {
            string[] sortOptions = new[] { "ShiftDate", "ShiftStart", "ShiftEnd" };
            if (EventID == null)
            {
                return NotFound("EventID is required.");
            }

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

            var shifts = _context.Shifts.Include(s => s.Event)
                .Where(e => e.EventID == EventID)
                .AsNoTracking();

            Event? thisEvent = await _context.Events
               .Include(s => s.Shifts)
               .AsNoTracking()
               .FirstOrDefaultAsync(s => s.ID == EventID);

            //Filters
            if (fromDate.HasValue)
                shifts = shifts.Where(d => d.ShiftDate >= fromDate);
            if (toDate.HasValue)
                shifts = shifts.Where(d => d.ShiftDate <= toDate.Value);

            #region Sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }
            if (sortField == "ShiftDate")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(p => p.ShiftDate)
                    : shifts.OrderByDescending(p => p.ShiftDate);
            }
            else if (sortField == "ShiftStart")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(v => v.ShiftStart)
                    : shifts.OrderByDescending(v => v.ShiftStart);
            }
            else if (sortField == "ShiftEnd")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(v => v.ShiftEnd)
                    : shifts.OrderByDescending(v => v.ShiftEnd);
            }
            #endregion
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
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

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

            ViewBag.EventID = shift.EventID;
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
        public async Task<IActionResult> Create([Bind("EventID,ShiftDate,ShiftStart,ShiftEnd")] Shift shift)
        {
            try
            {

                Event? thisEvent = await _context.Events
                    .Include(s => s.Shifts)
                    .FirstOrDefaultAsync(e => e.ID == shift.EventID);
                if (thisEvent == null)
                {
                    return NotFound();
                }

                shift.Event = thisEvent;


                if (ModelState.IsValid)
                {
                    _context.Add(shift);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { EventID = shift.EventID });
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.");
            }
            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shift);
        }

        // GET: EventShift/Edit/5
        public async Task<IActionResult> Edit(int id)
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
        public async Task<IActionResult> Edit(int id, Shift shift)
        {
            var shiftToUpdate = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (shiftToUpdate == null)
            {
                return NotFound();
            }

            // Ensure that the EventID is set correctly in the shiftToUpdate (even if it's not editable in the form)
            shiftToUpdate.EventID = shift.EventID;

            // Ensure that the event associated with the shift is updated if necessary
            Event? thisEvent = await _context.Events
                   .Include(s => s.Shifts)
                   .FirstOrDefaultAsync(e => e.ID == shift.EventID);
            if (thisEvent == null)
            {
                return NotFound();
            }
            shiftToUpdate.Event = thisEvent;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Successfully updated the Shift!";
                return RedirectToAction("Index", new { EventID = shiftToUpdate.EventID });
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


            ViewData["EventName"] = new SelectList(_context.Events, "ID", "EventCity", shift.EventID);
            return View(shiftToUpdate);
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
