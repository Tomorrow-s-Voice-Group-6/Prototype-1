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
        public async Task<IActionResult> Index(string? searchString, string? selDate, string? actionButton, string sortDirection = "asc", 
            string sortField = "Session")
        {
            string[] sortOptions = new[] { "Date", "Chapter" };

            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;
            var sessions = await _context.Sessions
                .Include(c => c.Chapter)
                .AsNoTracking()
                .Select(session => new AttendanceVM 
                //Create a new attendance vm, with the session and list of
                //singers who attended.
                {
                    Session = session,
                    Singers = session.SingerSessions.Select(s => s.Singer).ToList()
                }).OrderBy(s => s.Session.ID).ToListAsync();

            //Filters
            if (!String.IsNullOrEmpty(searchString)) 
            {
                sessions = sessions.Where(a => a.Session.Chapter.City.ToUpper()
                .Contains(searchString.ToUpper())).ToList();
                numFilters++;
            }
            if (!String.IsNullOrEmpty(selDate))
            {
                sessions = sessions.Where(a => a.Session.Date.ToShortDateString().Contains(selDate)).ToList();
                numFilters++;
            }
            //Feedback on filter button
            if(numFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numFilters"] = $"({numFilters.ToString()} Filter {(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = " show";
            }

            //Sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }
            //Sort fields
            if(sortField == "Date")
            {
                if(sortDirection == "asc")
                    sessions = sessions.OrderBy(s => s.Session.Date).ToList();
                else
                    sessions = sessions.OrderByDescending(s => s.Session.Date).ToList();
            }
            else if(sortField == "Chapter")
            {
                if (sortDirection == "asc")
                    sessions = sessions.OrderBy(s => s.Session.Chapter.City)
                        .ThenBy(s => s.Session.Chapter.ID).ToList();
                else
                    sessions = sessions.OrderByDescending(s => s.Session.Chapter.City)
                        .ThenBy(s => s.Session.Chapter.ID).ToList();
            }
            //Reset sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(sessions);
        }

        // GET: SingerSession/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //First, create the session and gather the data you need
            var sessions = await _context.SingerSessions
                .Include(s => s.Session).ThenInclude(c => c.Chapter)
                .Include(s => s.Singer)
                .Where(s => s.Session.ID == id)
                .AsNoTracking().ToListAsync();

            if (sessions == null)
            {
                return NotFound();
            }

            //Next, take the data and put it into a new VM to view summary details of the session and total singers
            var vm = new AttendanceVM 
            {
                Session = sessions.First().Session,
                Singers = sessions.Select(s => s.Singer).ToList()
            };

            

            return View(vm);
        }

        // GET: SingerSession/Create
        public IActionResult Create()
        {
            ViewData["SessionID"] = new SelectList(_context.Sessions, "ID", "ID");
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "FullName");
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
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "FullName", singerSession.SingerID);
            return View(singerSession);
        }

        // GET: SingerSession/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var sessions = await _context.SingerSessions
                .Include(s => s.Session).ThenInclude(c => c.Chapter)
                .Include(s => s.Singer)
                .Where(s => s.Session.ID == id)
                .AsNoTracking().ToListAsync();

            if (sessions == null)
            {
                return NotFound();
            }

            var vm = new AttendanceVM
            {
                Session = sessions.First().Session,
                Singers = sessions.Select(s => s.Singer).ToList(),
            };
            return View(vm);
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
            ViewData["SingerID"] = new SelectList(_context.Singers, "ID", "FullName", singerSession.SingerID);
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
