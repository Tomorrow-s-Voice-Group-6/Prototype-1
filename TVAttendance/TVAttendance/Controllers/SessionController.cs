﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;
using TVAttendance.ViewModels;
using TVAttendance.CustomControllers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TVAttendance.Controllers
{
    public class SessionController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public SessionController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Session
        public async Task<IActionResult> Index(
        int? ChapterID,
        string? DirectorName,
        DateTime? fromDate,
        DateTime? toDate,
        string? actionButton,
        string? fred,
        int? page = 1,
        int? pageSize = 10,
        string sortDirection = "asc",
        string sortField = "Date")
        {
            string[] sortOptions = new[] { "Date", "Chapter", "Director" };
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            var sessions = _context.Sessions
                .Include(s => s.Chapter)
                    .ThenInclude(c => c.Directors)
                .Include(s => s.Chapter)
                    .ThenInclude(c => c.Singers)
                .Include(s => s.SingerSessions)
                    .ThenInclude(ss => ss.Singer)
                .AsNoTracking();

            // Apply Filters
            if (ChapterID.HasValue)
            {
                sessions = sessions.Where(s => s.ChapterID == ChapterID.Value);
                numFilters++;
            }
            if (!string.IsNullOrEmpty(DirectorName))
            {
                sessions = sessions.Where(s => s.Chapter.Directors
                                                 .Any(d => (d.FirstName + " " + d.LastName).Contains(DirectorName) ||
                                                           (d.LastName + " " + d.FirstName).Contains(DirectorName)));
                numFilters++;
            }
            if (fromDate.HasValue && fromDate != new DateTime(2022, 1, 1))
            {
                sessions = sessions.Where(d => d.Date >= fromDate);
                numFilters++;
            }
            if (toDate.HasValue && toDate.Value != DateTime.Today)
            {
                sessions = sessions.Where(d => d.Date <= toDate.Value);
                numFilters++;
            }

            // Update UI for filters
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            else
            {
                ViewData["numFilters"] = "";
                ViewData["ShowFilter"] = "";
            }

            // Populate dropdowns
            ViewData["ChapterID"] = new SelectList(_context.Chapters.OrderBy(c => c.City), "ID", "City", ChapterID);
            ViewData["DirectorName"] = new SelectList(_context.Directors, "FullName", "FullName");

            if (fred == "Export")
            {
                return ExportData(fromDate, toDate);
            }

            // Sorting Logic
            if (!string.IsNullOrEmpty(actionButton) && sortOptions.Contains(actionButton))
            {
                page = 1;

                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }

                sortField = actionButton;
            }

            sessions = sortField switch
            {
                "Chapter" => sortDirection == "asc" ? sessions.OrderBy(s => s.Chapter.City) : sessions.OrderByDescending(s => s.Chapter.City),
                "Director" => sortDirection == "asc"
                    ? sessions.OrderBy(s => s.Chapter.Directors.Any() ? s.Chapter.Directors.First().LastName : "")
                              .ThenBy(s => s.Chapter.Directors.Any() ? s.Chapter.Directors.First().FirstName : "")
                    : sessions.OrderByDescending(s => s.Chapter.Directors.Any() ? s.Chapter.Directors.First().LastName : "")
                              .ThenByDescending(s => s.Chapter.Directors.Any() ? s.Chapter.Directors.First().FirstName : ""),
                _ => sortDirection == "asc" ? sessions.OrderBy(s => s.Date) : sessions.OrderByDescending(s => s.Date),
            };

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Pagination
            int actualPageSize = pageSize ?? 10;
            var pagedSessions = await PaginatedList<Session>.CreateAsync(sessions, page ?? 1, actualPageSize);

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = actualPageSize;
            ViewData["TotalPages"] = pagedSessions.TotalPages;
            ViewData["fromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["toDate"] = toDate?.ToString("yyyy-MM-dd");

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
                .Include(s => s.Chapter).ThenInclude(d => d.Directors)
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
            ViewData["ModalPopupSess"] = "hide";
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
                    TempData["SuccessMsg"] = "Successfully created a session!";
                    //return RedirectToAction("Details", new { session.ID });
                    ViewData["SessionID"] = session.ID;
                    ViewData["ModalPopupSess"] = "display";
                }
                else
                    TempData["ErrorMsg"] = "Error in creating a session. Please try again.";
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
                    TempData["SuccessMsg"] = "Successfully updated the session!";
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
            else
            {
                TempData["ErrorMsg"] = "Error in editing the session. Please try again.";
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
        //    if (session != null)D
        //    {
        //        _context.Sessions.Remove(session);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        public IActionResult ExportData(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null) { fromDate = DateTime.Now.AddMonths(-6); }
            if (toDate == null) { toDate = DateTime.Now; }

            var allSessions = _context.Sessions
                .Include(s => s.Chapter)
                .Include(s => s.SingerSessions)
                .Where(s => s.Date >= fromDate.Value && s.Date <= toDate.Value)
                .ToList();

            var export = new List<ExportFilterVM>();

            foreach (var session in allSessions)
            {
                export.Add(new ExportFilterVM
                {
                    chapter = session.Chapter.City,
                    startdate = session.Date.ToString("yyyy-MM-dd"),
                    attended = session.SingerSessions.Count()
                });
            }

            using (ExcelPackage excel = new ExcelPackage())
            {
                var workSheet = excel.Workbook.Worksheets.Add("Sessions");

                workSheet.Cells[1, 1].Value = "Attendance Summary Report";
                workSheet.Cells[1, 1, 1, 4].Merge = true;
                workSheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 1].Style.Font.Size = 16;
                workSheet.Cells[1, 1].Style.Font.Bold = true;

                workSheet.Cells[2, 1].Value = "Report Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                workSheet.Cells[2, 1, 2, 4].Merge = true;
                workSheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells[2, 1].Style.Font.Size = 12;

                workSheet.Cells[3, 1].Value = "Start Date:";
                workSheet.Cells[3, 2].Value = fromDate?.ToString("yyyy-MM-dd");
                workSheet.Cells[3, 3].Value = "End Date:";
                workSheet.Cells[3, 4].Value = toDate?.ToString("yyyy-MM-dd");

                workSheet.Cells[4, 1].Value = "Chapter:";
                workSheet.Cells[4, 2].Value = "Attended During Period:";
                workSheet.Cells[4, 2, 4, 3].Merge = true;

                workSheet.Cells[5, 1].Value = "Chapter";
                workSheet.Cells[5, 2].Value = "Date";
                workSheet.Cells[5, 3].Value = "Attendees";

                int count = 6;
                foreach (var record in export)
                {
                    workSheet.Cells[count, 1].Value = record.chapter;
                    workSheet.Cells[count, 2].Value = record.startdate;
                    workSheet.Cells[count, 3].Value = record.attended;
                    count++;
                }

                workSheet.Cells.AutoFitColumns();

                try
                {
                    Byte[] data = excel.GetAsByteArray();
                    string fileName = "Sessions.xlsx";
                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    TempData["SuccessMsg"] = "Successfully built file. Will begin download shortly...";
                    return File(data, mimeType, fileName);
                }
                catch (Exception)
                {
                    TempData["ErrorMsg"] = "Could not build and download the file.";
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
