using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;

namespace TVAttendance.Controllers
{
    public class VolunteerEventController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerEventController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: VolunteerEvent
        public async Task<IActionResult> Index()
        {
            var tomorrowsVoiceContext = _context.VolunteerEvents.Include(v => v.Event).Include(v => v.Volunteer);
            return View(await tomorrowsVoiceContext.ToListAsync());
        }

        // GET: VolunteerEvent/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerEvent = await _context.VolunteerEvents
                .Include(v => v.Event)
                .Include(v => v.Volunteer)
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (volunteerEvent == null)
            {
                return NotFound();
            }

            return View(volunteerEvent);
        }

        // GET: VolunteerEvent/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity");
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email");
            return View();
        }

        // POST: VolunteerEvent/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,VolunteerID,ShiftAttended,ShiftStart,ShiftEnd,NonAttendanceNote")] VolunteerEvent volunteerEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(volunteerEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }

        // GET: VolunteerEvent/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerEvent = await _context.VolunteerEvents.FindAsync(id);
            if (volunteerEvent == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }

        // POST: VolunteerEvent/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,VolunteerID,ShiftAttended,ShiftStart,ShiftEnd,NonAttendanceNote")] VolunteerEvent volunteerEvent)
        {
            if (id != volunteerEvent.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(volunteerEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VolunteerEventExists(volunteerEvent.EventID))
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
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }

        // GET: VolunteerEvent/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerEvent = await _context.VolunteerEvents
                .Include(v => v.Event)
                .Include(v => v.Volunteer)
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (volunteerEvent == null)
            {
                return NotFound();
            }

            return View(volunteerEvent);
        }

        // POST: VolunteerEvent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var volunteerEvent = await _context.VolunteerEvents.FindAsync(id);
            if (volunteerEvent != null)
            {
                _context.VolunteerEvents.Remove(volunteerEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VolunteerEventExists(int id)
        {
            return _context.VolunteerEvents.Any(e => e.EventID == id);
        }
    }
}
