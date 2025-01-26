﻿using System;
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
        string sortDirection = "desc",
        string sortField = "Date",
        int page = 1,
        int pageSize = 15)
        {
            string[] sortOptions = new[] { "Notes", "Chapter", "DirectorID" };
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
                sessions = sessions.Where(s => s.Chapter.Director.LastName.ToUpper().Contains(DirectorName.ToUpper())
                                               || s.Chapter.Director.FirstName.ToUpper().Contains(DirectorName.ToUpper()));
                numFilters++;
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
                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            sessions = sortField switch
            {
                "Notes" => sortDirection == "asc" ? sessions.OrderBy(s => s.Notes) : sessions.OrderByDescending(s => s.Notes),
                "ChapterID" => sortDirection == "asc" ? sessions.OrderBy(s => s.Chapter.City) : sessions.OrderByDescending(s => s.Chapter.City),
                "DirectorID" => sortDirection == "asc"
                    ? sessions.OrderBy(s => s.Chapter.Director.LastName).ThenBy(s => s.Chapter.Director.FirstName)
                    : sessions.OrderByDescending(s => s.Chapter.Director.LastName).ThenBy(s => s.Chapter.Director.FirstName),
                _ => sortDirection == "asc" ? sessions.OrderBy(s => s.Date) : sessions.OrderByDescending(s => s.Date)
            };
            #endregion

            // Pages
            var totalItems = await sessions.CountAsync();
            var pagedSessions = await sessions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Data for Paging and Sorting
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Populate dropdowns
            PopulateDDLs();

            return View(pagedSessions);
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

        public IActionResult DownloadAttendance()
        {
            var attend = from a in _context.SingerSessions
                        .Include(s => s.Singer)
                        .Include(s => s.Session)
                        .ThenInclude(s => s.Date)
                         orderby a.Session.Date descending
                         select new
                         {
                             Date = a.Session.Date.ToShortDateString(),
                             Attendees = a.Session.SingerSessions.Count()
                         };

            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Attendance");

                // Add title
                worksheet.Cells[1, 1].Value = "Attendance Report";
                worksheet.Cells[1, 1, 1, 6].Merge = true; // Merge cells for title
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;

                // Add current date and time 
                worksheet.Cells[2, 1].Value = "Report Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[2, 1, 2, 6].Merge = true; // Merge cells for date/time
                worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;

                // Add Columns 
                worksheet.Cells[4, 1].Value = "Date";

                // Add data rows
                int row = attend.Count();

                foreach (var item in attend)
                {
                    worksheet.Cells[row, 1].Value = item.Date;
                    row++;
                }

                // AutoFit columns to content
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


                try
                {
                    Byte[] theData = excel.GetAsByteArray();
                    string filename = "Attendance.xlsx";
                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(theData, mimeType, filename);
                }
                catch (Exception)
                {
                    return BadRequest("Could not build and download the file.");
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
