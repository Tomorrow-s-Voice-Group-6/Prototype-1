using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;
using TVAttendance.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> Index(
        int? ChapterID,
        string? startDate,
        string? endDate,
        string? actionButton,
        string sortDirection = "asc",
        string sortField = "Session",
        int page = 1,
        int pageSize = 10)
        {
            string[] sortOptions = new[] { "Date", "Chapter" };

            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            // Get sessions with related entities
            var sessionsQuery = _context.Sessions
                .Include(c => c.Chapter)
                .AsNoTracking()
                .Select(session => new AttendanceVM
                {
                    Session = session,
                    Singers = session.SingerSessions.Select(s => s.Singer).ToList()
                });
            PopulateDDLs();
            // Filters
            if (ChapterID.HasValue)
            {
                sessionsQuery = sessionsQuery.Where(a => a.Session.ChapterID.Equals(ChapterID));
                numFilters++;
            }
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate)) //Filter by RANGE
            { 
                sessionsQuery = sessionsQuery.Where(a => a.Session.Date >= DateTime.Parse(startDate)
                                && a.Session.Date <= DateTime.Parse(endDate));
                numFilters++;
            }
            else if (!String.IsNullOrEmpty(startDate)) //Filter by ONLY start date
            {
                sessionsQuery = sessionsQuery.Where(a => a.Session.Date >= DateTime.Parse(startDate));
                numFilters++;
            }
            else if (!String.IsNullOrEmpty(endDate)) //Filter by ONLY end date
            {
                sessionsQuery = sessionsQuery.Where(a => a.Session.Date <= DateTime.Parse(endDate));
                numFilters++;
            }


            // Feedback on filter button
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters.ToString()} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }

            // Sorting
            if (!String.IsNullOrEmpty(actionButton) && sortOptions.Contains(actionButton))
            {
                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            // Sort fields
            switch (sortField)
            {
                case "Date":
                    sessionsQuery = sortDirection == "asc"
                        ? sessionsQuery.OrderBy(s => s.Session.Date)
                        : sessionsQuery.OrderByDescending(s => s.Session.Date);
                    break;
                case "Chapter":
                    sessionsQuery = sortDirection == "asc"
                        ? sessionsQuery.OrderBy(s => s.Session.Chapter.City)
                            .ThenBy(s => s.Session.Chapter.ID)
                        : sessionsQuery.OrderByDescending(s => s.Session.Chapter.City)
                            .ThenBy(s => s.Session.Chapter.ID);
                    break;
                default:
                    sessionsQuery = sortDirection == "asc"
                        ? sessionsQuery.OrderBy(s => s.Session.ID)
                        : sessionsQuery.OrderByDescending(s => s.Session.ID);
                    break;
            }

            // Paging
            var totalItems = await sessionsQuery.CountAsync();
            
            var sessions = await PaginatedList<AttendanceVM>.CreateAsync(sessionsQuery, page, pageSize);
            // Data for Paging and Sorting
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
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

            if (!sessions.Any())
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
        private SelectList ChapterSelectList(int? selID)
        {
            return new SelectList(_context.Chapters
               .OrderBy(c => c.ID), "ID", "City", selID);
        }
        private void PopulateDDLs(SingerSession? singerSession = null)
        {
            ViewData["Attendees"] = Attendees(singerSession?.SessionID);
            ViewData["ChapterID"] = ChapterSelectList(singerSession?.SessionID);
        }
        private bool SingerSessionExists(int id)
        {
            return _context.SingerSessions.Any(e => e.SingerID == id);
        }
    }
}
