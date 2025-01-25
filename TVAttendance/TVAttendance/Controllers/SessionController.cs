using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class SessionController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public SessionController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Session
        public async Task<IActionResult> Index(int? ChapterID, string? DirectorName, string? searchString, string? actionButton,
            string sortDirection = "desc", string sortField = "Date")
        { //Default sort is date by desc order
            string[] sortOptions = new[] { "Notes", "Chapter", "Director", "Date" };

            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;
            //get the session
            var sessions = _context.Sessions
                .Include(s => s.Chapter).ThenInclude(d => d.Director)
                .Include(s => s.SingerSessions).ThenInclude(s => s.Singer)
                .AsNoTracking();
            #region Filters
            if (ChapterID.HasValue)
            {
                sessions = sessions.Where(s => s.ChapterID == ChapterID.Value);
                numFilters++;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                if (DateTime.TryParse(searchString, out DateTime date)) //entire date parse
                {
                    sessions = sessions.Where(s => s.Date.Date == date.Date);
                }
                else if (int.TryParse(searchString, out int month)) //month parse
                {
                    sessions = sessions.Where(s => s.Date.Month == month);
                }
                else //any other filter
                {
                    sessions = sessions.Where(s => s.Date.ToShortDateString().Contains(searchString));
                }
                numFilters++;
            }
            if (!String.IsNullOrEmpty(DirectorName))
            {
                sessions = sessions.Where(s => s.Chapter.Director.LastName.ToUpper().Contains(DirectorName.ToUpper())
                                       || s.Chapter.Director.FirstName.ToUpper().Contains(DirectorName.ToUpper()));
                numFilters++;
            }
            if (numFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";
                ViewData["numFilters"] = $"({numFilters.ToString()} Filter {(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = " show";
            }
            #endregion
            #region Sorting
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
            if (sortField == "Notes")
            {
                if (sortDirection == "asc")
                    sessions = sessions.OrderBy(s => s.Notes);
                else
                    sessions = sessions.OrderByDescending(s => s.Notes);
            }
            else if (sortField == "Chapter")
            {
                if (sortDirection == "asc")
                    sessions = sessions.OrderBy(s => s.Chapter.City);
                else
                    sessions = sessions.OrderByDescending(s => s.Chapter.City);
            }
            else if (sortField == "Director")
            {
                if (sortDirection == "asc")
                {
                    sessions = sessions.OrderBy(s => s.Chapter.Director.LastName)
                        .ThenBy(s => s.Chapter.Director.FirstName);
                }
                else
                {
                    sessions = sessions.OrderByDescending(s => s.Chapter.Director.LastName)
                        .ThenBy(s => s.Chapter.Director.FirstName);
                }            
            }
            else if(sortField == "Date") //Default sort by date
            {
                if(sortDirection == "desc")
                {
                    sessions = sessions.OrderByDescending(s => s.Date);
                }
                else
                {
                    sessions = sessions.OrderBy(s => s.Date);
                }
            }
            #endregion

            //Reset sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            PopulateDDLs();
            return View(sessions.ToList());
        }

        // GET: Session/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Chapter)
                .Include(s => s.SingerSessions).ThenInclude(a => a.Singer)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (session == null)
            {
                return NotFound();
            }
            PopulateDDLs(session);
            return View(session);
        }

        // GET: Session/Create
        public IActionResult Create()
        {
            Session session = new Session();
            PopualteAssignedSingers(session);
            PopulateDDLs(session);
            return View(session);
        }

        // POST: Session/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Notes,Date,ChapterID")] Session session, string[] selectedOpts)
        {
            try
            {
                UpdateSingersAttended(selectedOpts, session);
                if (ModelState.IsValid)
                {
                    _context.Add(session);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new {session.ID});
                }
            }
            catch (RetryLimitExceededException ex)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts." +
                    " Try again, and if the problem persists, see your system administrator.");
            }
            PopualteAssignedSingers(session);
            PopulateDDLs(session);
            return View(session);
        }

        // GET: Session/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Chapter)
                .Include(s => s.SingerSessions).ThenInclude(a => a.Singer)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (session == null)
            {
                return NotFound();
            }
            PopulateDDLs(session);
            PopualteAssignedSingers(session);
            return View(session);
        }

        // POST: Session/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Notes,Date,ChapterID,Chapter")] Session session,
            string[] selectedOpts)
        {
            
            var sessionToUpdate = await _context.Sessions
                .Include(s => s.Chapter)
                .Include(s => s.SingerSessions).ThenInclude(s => s.Singer)
                .FirstOrDefaultAsync(s => s.ID == id);
            
            if (sessionToUpdate == null) { return NotFound(); }

            //Update the singers
            UpdateSingersAttended(selectedOpts, sessionToUpdate);
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            sessionToUpdate.Chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.ID == sessionToUpdate.ChapterID);
            if (await TryUpdateModelAsync<Session>(sessionToUpdate, "",
                s=> s.Notes, s=> s.Date, s=>s.Chapter, s=>s.ChapterID))
            {

                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", new { sessionToUpdate.ID });
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts." +
                        " Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, " +
                        "and if the problem persists see your system administrator.");
                }
            }
            PopulateDDLs(sessionToUpdate);
            PopualteAssignedSingers(sessionToUpdate);
            return View(sessionToUpdate);
        }

        // GET: Session/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Session/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopualteAssignedSingers(Session session)
        {
            var all = _context.Singers; //First get all singers and create a hashset of the current singers
            var current = new HashSet<int>(session.SingerSessions.Select(s => s.SingerID));
            //Next setup both select lists

            var selectedOpts = new List<ListOptionVM>();
            var availableOpts = new List<ListOptionVM>();
            foreach (var s in all)
            {
                if (current.Contains(s.ID)) //if the list already contains the singer
                {
                    selectedOpts.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        Text = s.FullName
                    });
                }
                else //otherwise make the singer available to add
                {
                    availableOpts.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        Text = s.FullName
                    });
                }
            }
            ViewData["selOpts"] = new MultiSelectList(selectedOpts.OrderBy(s => s.Text), "ID", "Text");
            ViewData["availOpts"] = new MultiSelectList(availableOpts.OrderBy(s => s.Text), "ID", "Text");
        }
        
        private void UpdateSingersAttended(string[] selected, Session sessionToUpdate)
        {
            if(selected == null)
            {
                sessionToUpdate.SingerSessions = new List<SingerSession>();
                return;
            }
            //Get all singers selected
            var selectedSingers = new HashSet<string>(selected);
            //Get the current singers for the selected session
            var current = new HashSet<int>(sessionToUpdate.SingerSessions.Select(s => s.SingerID));

            foreach (var s in _context.Singers) { //where s is each singer in the singer database
                if (selectedSingers.Contains(s.ID.ToString())) //if its selected
                {
                    if (!current.Contains(s.ID)) //but not in the current collection
                    {
                        sessionToUpdate.SingerSessions.Add(new SingerSession
                        {
                            SessionID = sessionToUpdate.ID,
                            SingerID = s.ID,
                            Notes = $"Attendance Record for {s.FullName}"
                        });
                    }
                }
                else //not selected
                {
                    if (current.Contains(s.ID))
                    {
                        var singerToRemove = sessionToUpdate.SingerSessions.FirstOrDefault(a => a.SingerID == s.ID);

                        if(singerToRemove != null) //remove the singer from the SingerSession (attendance) list
                        {
                            _context.SingerSessions.Remove(singerToRemove);
                        }
                    }
                }
            }
        }

        private SelectList ChapterSelectList(int? selID)
        {
            return new SelectList(_context.Chapters
                .OrderBy(c => c.ID), "ID", "City", selID);
        }
        private SelectList DirectorSelectList(int? selID)
        {
            return new SelectList(_context.Directors
                .ToList()
                .OrderBy(d => d.FullName), "ID", "FullName", selID);
        }
        private void PopulateDDLs(Session? session = null)
        {
            ViewData["ChapterID"] = ChapterSelectList(session?.ChapterID);
            ViewData["DirectorID"] = DirectorSelectList(session?.Chapter?.DirectorID);
        }
        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.ID == id);
        }
    }
}
