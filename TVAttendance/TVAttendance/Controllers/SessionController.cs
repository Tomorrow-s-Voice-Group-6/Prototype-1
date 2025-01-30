using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;
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
        // GET: Session
        public async Task<IActionResult> Index(
        int? ChapterID,
        string? DirectorName,
        string? searchString,
        string? actionButton,
        int? page = 1,
        string sortDirection = "asc",
        string sortField = "Date")
        {
            string[] sortOptions = new[] { "Date", "Chapter", "Director" };
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

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
            if (!string.IsNullOrEmpty(searchString))
            {
                sessions = sessions.Where(a => a.Date.ToShortDateString().Contains(searchString));
                numFilters++;
            }
            if (!string.IsNullOrEmpty(DirectorName))
            {
                if (int.TryParse(DirectorName, out int directorId)) // Convert string to int safely
                {
                    sessions = sessions.Where(s => s.Chapter.Director.ID == directorId);
                    numFilters++;
                }
            }

            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            #endregion

            #region Sorting
            if (!string.IsNullOrEmpty(actionButton) && sortOptions.Contains(actionButton))
            {
                page = 1;

                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            switch (sortField)
            {
                case "Chapter":
                    sessions = sortDirection == "asc"
                        ? sessions.OrderBy(s => s.Chapter.City)
                        : sessions.OrderByDescending(s => s.Chapter.City);
                    break;
                case "Director":
                    sessions = sortDirection == "asc"
                        ? sessions.OrderBy(s => s.Chapter.Director.LastName)
                            .ThenBy(s => s.Chapter.Director.FirstName)
                        : sessions.OrderByDescending(s => s.Chapter.Director.LastName)
                            .ThenByDescending(s => s.Chapter.Director.FirstName);
                    break;
                case "Date":
                    sessions = sortDirection == "asc"
                        ? sessions.OrderBy(s => s.Date.Year).ThenBy(s => s.Date.Month).ThenBy(s => s.Date.Day)
                        : sessions.OrderByDescending(s => s.Date.Year)
                            .ThenByDescending(s => s.Date.Month).ThenByDescending(s => s.Date.Day);
                    break;
                default:
                    sessions = sortDirection == "asc"
                        ? sessions.OrderBy(s => s.Date.Year).ThenBy(s => s.Date.Month).ThenBy(s => s.Date.Day)
                        : sessions.OrderByDescending(s => s.Date.Year)
                            .ThenByDescending(s => s.Date.Month).ThenByDescending(s => s.Date.Day);
                    break;
            }
            #endregion

            // Pages
            var totalItems = await sessions.CountAsync();
            int pageSize = 10;
            var pagedData = await PaginatedList<Session>.CreateAsync(sessions.AsNoTracking(), page ?? 1, pageSize);

            // Populate dropdowns
            PopulateDDLs();
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(pagedData);
        }


        // GET: Session/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Chapter).ThenInclude(d => d.Director)
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
            PopulateAssignedSingers(session, 0);
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
                    return RedirectToAction("Details", new { session.ID });
                }
            }
            catch (RetryLimitExceededException ex)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts." +
                    " Try again, and if the problem persists, see your system administrator.");
            }
            PopulateAssignedSingers(session, 0);
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
            PopulateAssignedSingers(session, session.ChapterID);
            return View(session);
        }

        // POST: Session/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Notes,Date,ChapterID")] Session session,
            string[] selectedOpts)
        {

            var sessionToUpdate = await _context.Sessions
                .Include(s => s.Chapter)
                .Include(s => s.Chapter.Director)
                .Include(s => s.SingerSessions).ThenInclude(s => s.Singer)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (sessionToUpdate == null) { return NotFound(); }

            //Update the singers
            UpdateSingersAttended(selectedOpts, sessionToUpdate);

            if (await TryUpdateModelAsync<Session>(sessionToUpdate, "",
                s => s.Notes, s => s.Date, s => s.Chapter, s => s.ChapterID))
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
            PopulateAssignedSingers(sessionToUpdate, session.ChapterID);
            return View(sessionToUpdate);
        }

        // GET: Session/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var session = await _context.Sessions
        //        .Include(s => s.Chapter)
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (session == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(session);
        //}

        //// POST: Session/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var session = await _context.Sessions.FindAsync(id);
        //    if (session != null)
        //    {
        //        _context.Sessions.Remove(session);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        public IActionResult DownloadSessions()
        {
            var sess = from s in _context.Sessions
                       select s;
            var chap = from c in _context.Chapters
                       select c;

            using (ExcelPackage excel = new ExcelPackage())
            {
                var workSheet = excel.Workbook.Worksheets.Add("Sessions");

                int count = 1;
                int countChapCell = 1;

                foreach (var c in chap)
                {
                    var attendance = from s in sess
                                     where s.ChapterID == c.ID
                                     select new
                                     {
                                         SessionDate = s.Date.ToShortDateString(),
                                         Attendees = s.SingerSessions.Select(s => s.SingerID).Count()
                                     };

                    workSheet.Cells[1, countChapCell].Value = c.City;
                    workSheet.Cells[3, count].LoadFromCollection(attendance, true);

                    countChapCell = countChapCell + 3;
                    count = count + 3;
                }
                countChapCell = 0;
                count = 0;

                workSheet.Cells.AutoFitColumns();

                try
                {
                    Byte[] data = excel.GetAsByteArray();
                    string fileName = "Sessions.xlsx";
                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(data, mimeType, fileName);
                }
                catch (Exception)
                {
                    return BadRequest("Could not build and download the file");
                }
            }
        }

        private void PopulateAssignedSingers(Session session, int? chapterId)
        {
            // Get chapter ID or use default
            var chapter = chapterId ?? (int?)ViewBag.ChapterID;

            // Get all active singers
            var allSingers = _context.Singers.ToList();

            // Get currently assigned singers
            var currentSingersInSession = new HashSet<int>(session.SingerSessions.Select(s => s.SingerID));

            // Initialize lists for selected and available singers
            var selectedSingers = new List<ListOptionVM>();
            var availableSingers = new List<ListOptionVM>();

            // Sort singers by chapter
            foreach (var singer in allSingers)
            {
                // Check if active and in correct chapter
                if (singer.Status == true && singer.ChapterID == chapter)
                {
                    var listOption = new ListOptionVM
                    {
                        ID = singer.ID,
                        Text = singer.FullName
                    };

                    // Assign to selected or available
                    if (currentSingersInSession.Contains(singer.ID))
                    {
                        selectedSingers.Add(listOption);
                    }
                    else
                    {
                        availableSingers.Add(listOption);
                    }
                }
            }

            // Store available and selected singers in ViewData
            ViewData["selOpts"] = new MultiSelectList(selectedSingers.OrderBy(s => s.Text), "ID", "Text");
            ViewData["availOpts"] = new MultiSelectList(availableSingers.OrderBy(s => s.Text), "ID", "Text");

            // Get all chapters
            var allChapters = _context.Chapters.ToList();

            // Prepare singer data by chapter
            var chapterData = new Dictionary<int, List<ListOptionVM>>();

            // Loop through each chapter
            foreach (var chap in allChapters)
            {
                // List for singers in this chapter
                var chapterAvailableSingers = new List<ListOptionVM>();

                // Add singers for the chapter
                foreach (var singer in allSingers)
                {
                    if (singer.Status == true && singer.ChapterID == chap.ID)
                    {
                        chapterAvailableSingers.Add(new ListOptionVM
                        {
                            ID = singer.ID,
                            Text = singer.FullName
                        });
                    }
                }

                // Store chapter singer list in ViewData
                ViewData[$"chapter_{chap.ID}_availOpts"] = chapterAvailableSingers;
            }
        }

        private void UpdateSingersAttended(string[] selected, Session sessionToUpdate)
        {
            if (selected == null)
            {
                sessionToUpdate.SingerSessions = new List<SingerSession>();
                return;
            }
            //Get all singers selected
            var selectedSingers = new HashSet<string>(selected);
            //Get the current singers for the selected session
            var current = new HashSet<int>(sessionToUpdate.SingerSessions.Select(s => s.SingerID));

            foreach (var s in _context.Singers)
            { //where s is each singer in the singer database
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

                        if (singerToRemove != null) //remove the singer from the SingerSession (attendance) list
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
