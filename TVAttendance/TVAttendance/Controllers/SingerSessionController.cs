using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class SingerSessionController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public SingerSessionController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: SingerSession
        public async Task<IActionResult> Index()
        {
            var sessions = await _context.Sessions
                .Include(c => c.Chapter)
                .Select(session => new AttendanceVM 
                //Create a new attendance vm, with the session and list of
                //singers who attended.

                {
                    Session = session,
                    Singers = session.SingerSessions.Select(s => s.Singer).ToList()
                }).OrderBy(s => s.Session.ID).ToListAsync();
            
            return View(sessions);
        }

        // GET: SingerSession/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singerSession = await _context.SingerSessions
                .Include(s => s.Session).ThenInclude(s => s.Chapter)
                .Include(s => s.Singer)
                .FirstOrDefaultAsync(m => m.SingerID == id);
            if (singerSession == null)
            {
                return NotFound();
            }

            return View(singerSession);
        }

        // GET: SingerSession/Create
        public IActionResult Create()
        {
            ViewData["SessionID"] = new SelectList(_context.Sessions, "ID", "ID");
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "EmergencyContactPhone");
            return View();
        }

        // POST: SingerSession/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SingerID,SessionID,Notes")] SingerSession singerSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(singerSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SessionID"] = new SelectList(_context.Sessions, "ID", "ID", singerSession.SessionID);
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "EmergencyContactPhone", singerSession.SingerID);
            return View(singerSession);
        }

        // GET: SingerSession/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singerSession = await _context.SingerSessions.FindAsync(id);
            if (singerSession == null)
            {
                return NotFound();
            }
            ViewData["SessionID"] = new SelectList(_context.Sessions, "ID", "ID", singerSession.SessionID);
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "EmergencyContactPhone", singerSession.SingerID);
            return View(singerSession);
        }

        // POST: SingerSession/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SingerID,SessionID,Notes")] SingerSession singerSession)
        {
            if (id != singerSession.SingerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(singerSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SingerSessionExists(singerSession.SingerID))
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
            ViewData["SessionID"] = new SelectList(_context.Sessions, "ID", "ID", singerSession.SessionID);
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "EmergencyContactPhone", singerSession.SingerID);
            return View(singerSession);
        }

        // GET: SingerSession/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singerSession = await _context.SingerSessions
                .Include(s => s.Session)
                .Include(s => s.Singer)
                .FirstOrDefaultAsync(m => m.SingerID == id);
            if (singerSession == null)
            {
                return NotFound();
            }

            return View(singerSession);
        }

        // POST: SingerSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var singerSession = await _context.SingerSessions.FindAsync(id);
            if (singerSession != null)
            {
                _context.SingerSessions.Remove(singerSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private SelectList Attendees(int? selID)
        {
            var qry = _context.SingerSessions
                .Where(s => s.SessionID == selID)
                .Select(s => s.Singer)
                .OrderBy(s => s.ChapterID).ToList();
            return new SelectList(qry, "ID", "FullName", selID);
        }
        private void PopulateDDLs(SingerSession? singerSession = null)
        {
            ViewData["Attendees"] = Attendees(singerSession?.SessionID);
        }
        private bool SingerSessionExists(int id)
        {
            return _context.SingerSessions.Any(e => e.SingerID == id);
        }
    }
}
