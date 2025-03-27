using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.CustomControllers;
using TVAttendance.Utilities;
using Microsoft.AspNetCore.Authorization;
using TVAttendance.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Microsoft.AspNetCore.Http;

namespace TVAttendance.Controllers
{
    public class DirectorController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public DirectorController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Director
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Index(string? SearchString, bool showArchived = false,
            int? page = 1,
            int? pageSize = 15,
            int? chapterId = null
            )
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            var director = _context.Directors
                .Include(s => s.Chapters)
                .AsNoTracking();

            if (showArchived)
            {
                director = director.Where(d => !d.Status); // Show only archived directors
            }
            else
            {
                director = director.Where(d => d.Status); // Show only active directors
            }

            // If a Chapter is selected, filter directors associated with that Chapter
            if (chapterId.HasValue)
            {
                director = director.Where(d => d.Chapters.Any(c => c.ID == chapterId.Value));
                numFilters++;
                ViewData["SelectedChapter"] = chapterId.Value;
            }
            else
            {
                ViewData["SelectedChapter"] = null;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                director = director.Where(s => s.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numFilters++;
            }

            int actualPageSize = pageSize ?? 10;
            var pagedDirectors = await PaginatedList<Director>.CreateAsync(director, page ?? 1, actualPageSize);

            ViewData["CurrentPage"] = page ?? 1;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = pagedDirectors.TotalPages;
            ViewData["ShowArchived"] = showArchived;

            // Load chapters for the dropdown.
            // Here I'm using "City" as the display text, but you can change it as needed.
            ViewData["ChapterList"] = new SelectList(_context.Chapters, "ID", "City");

            return View(pagedDirectors); 
        }

        // GET: Director/Details/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Directors
                .Include(s => s.Chapters)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (director == null)
            {
                return NotFound();
            }

            return View(director);
        }

        // GET: Director/Create
        [Authorize(Roles = "Supervisor, Admin")]
        public IActionResult Create()
        {
            ViewData["ModalPopupdir"] = "hide";
            Director director = new Director();
            PopulateAssignedChapters(director);
            PopulateLists();
            return View(director);
        }

        // POST: Director/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Email,Phone,Status")] Director director, string[] selected)
        {
            try
            {
                UpdateDirectorChapters(selected, director);
                if (ModelState.IsValid)
                {
                    _context.Add(director);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = $"Successfully created {director.FullName}!";
                    ViewData["ModalPopupdir"] = "show";
                }

            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMsg"] = "Error in creating a Director. Please try again or contact the administrator.";
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Directors.Email"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
                        "you cannot have duplicate Emails."); 
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateAssignedChapters(director);
            PopulateLists(director);
            return View(director);
        }

        // GET: Director/Edit/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Directors
                .Include(s => s.Chapters)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ID == id);

            if (director == null)
            {
                return NotFound();
            }

            // Get logged-in user's email
            var userEmail = User.Identity?.Name;

            // If the logged-in user is a Director, they can only edit their own profile
            if (User.IsInRole("Director") && !string.Equals(director.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid(); // 403 Forbidden if trying to edit someone else
            }
            PopulateAssignedChapters(director);
            PopulateLists();
            return View(director);
        }

        // POST: Director/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, string[] selected)
        {
            var directorToUpdate = await _context.Directors
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (directorToUpdate == null)
            {
                return NotFound();
            }

            // Get logged-in user's email
            var userEmail = User.Identity?.Name;

            // If the logged-in user is a Director, they can only edit their own profile
            if (User.IsInRole("Director") && !string.Equals(directorToUpdate.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid(); // 403 Forbidden if trying to edit someone else
            }
            UpdateDirectorChapters(selected, directorToUpdate);
            if (await TryUpdateModelAsync<Director>(directorToUpdate, "",
                d => d.FirstName, d => d.LastName, d => d.DOB, d => d.Address,
                d => d.Email, d => d.Phone))
            {
                try
                {
                    _context.Update(directorToUpdate);
                    await _context.SaveChangesAsync();
                    if (ModelState.IsValid)
                    {
                        TempData["SuccessMsg"] = $"Successfully updated Director: {directorToUpdate.FirstName} {directorToUpdate.LastName}!";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["ErrorMsg"] = "Error in updating the Director. Please try again or contact the administrator.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DirectorExists(directorToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            PopulateAssignedChapters(directorToUpdate);
            PopulateLists(directorToUpdate);
            return View(directorToUpdate);
        }


        //// GET: Director/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var director = await _context.Directors
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (director == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(director);
        //}

        //// POST: Director/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var director = await _context.Directors.FindAsync(id);
        //    if (director != null)
        //    {
        //        _context.Directors.Remove(director);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Archive(int id)
        {
            var directorToUpdate = await _context.Directors
            .FirstOrDefaultAsync(d => d.ID == id);

            if (directorToUpdate == null)
            {
                return NotFound();
            }
            directorToUpdate.Status = false;

            try
            {
                _context.Update(directorToUpdate);
                await _context.SaveChangesAsync();
                if (ModelState.IsValid)
                {
                    TempData["SuccessMsg"] = $"Successfully archived Director: {directorToUpdate.FullName}.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMsg"] = "Error in archiving the Director.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMsg"] = "Error in archiving Director. Database concurrency update error";
                if (!DirectorExists(directorToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            //Only happens with an error
            return View(directorToUpdate);
        }

        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var directorToUpdate = await _context.Directors
            .FirstOrDefaultAsync(d => d.ID == id);

            if (directorToUpdate == null)
            {
                return NotFound();
            }
            directorToUpdate.Status = true;

            try
            {
                _context.Update(directorToUpdate);
                await _context.SaveChangesAsync();
                if (ModelState.IsValid)
                {
                    TempData["SuccessMsg"] = $"Successfully Restored {directorToUpdate.FullName}";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMsg"] = "Error in restoring the Director";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DirectorExists(directorToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return View(directorToUpdate);
        }

        private void PopulateAssignedChapters(Director director)
        {
            var allChapters = _context.Chapters.Include(s => s.Directors).ToList();

            var currentAssigendChapters = new HashSet<int>(director.Chapters.Select(c => c.ID));

            var selChapters = new List<ListOptionVM>();
            var availChapters = new List<ListOptionVM>();


            foreach (var chapter in allChapters)
            {
                var listOption = new ListOptionVM
                {
                    ID = chapter.ID,
                    Text = chapter.City
                };
                // Assign to selected or available
                if (currentAssigendChapters.Contains(chapter.ID))
                {
                    selChapters.Add(listOption);
                }
                else
                {
                    availChapters.Add(listOption);
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selChapters.OrderBy(s => s.Text), "ID", "Text");
            ViewData["availOpts"] = new MultiSelectList(availChapters.OrderBy(s => s.Text), "ID", "Text");
        }
        private void UpdateDirectorChapters(string[] selected, Director directorToUpdate)
        {
            if (selected == null)
            {
                directorToUpdate.Chapters = new List<Chapter>();
                return;
            }

            // Get selected chapter IDs as a HashSet
            var selectedChapterIds = new HashSet<int>(selected.Select(int.Parse));

            // Get currently assigned chapters
            var currentChapters = new HashSet<int>(directorToUpdate.Chapters.Select(c => c.ID));

            foreach (var chapter in _context.Chapters)
            {
                if (selectedChapterIds.Contains(chapter.ID))
                {
                    // Add if not already assigned
                    if (!currentChapters.Contains(chapter.ID))
                    {
                        directorToUpdate.Chapters.Add(chapter);
                    }
                }
                else
                {
                    // Remove if it was previously assigned but no longer is
                    var chapterToRemove = directorToUpdate.Chapters.FirstOrDefault(c => c.ID == chapter.ID);
                    if (chapterToRemove != null)
                    {
                        directorToUpdate.Chapters.Remove(chapterToRemove);
                    }
                }
            }
        }

        private SelectList DirectorList(int? selectedId)
        {
            return new SelectList(_context.Directors
                .OrderBy(d => d.FullName), "ID", "FullName", selectedId);
        }
        private void PopulateLists(Director director = null)
        {
            ViewData["FullName"] = DirectorList(director?.ID);
            ViewBag.Chapters = new SelectList(_context.Chapters.OrderBy(m => m.City), "ID", "City", director?.Chapters);
        }

        private bool DirectorExists(int id)
        {
            return _context.Directors.Any(e => e.ID == id);
        }
    }
}
