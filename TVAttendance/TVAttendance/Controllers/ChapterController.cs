using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class ChapterController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public ChapterController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // Modified to include Director Name Filter
        public async Task<IActionResult> Index(string locationFilter, string directorSearch, int? page = 1, int? pageSize = 10)
        {
            var locations = await _context.Chapters
                .Where(c => !string.IsNullOrEmpty(c.City))
                .Select(c => c.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            locations.Insert(0, "All Locations");
            ViewBag.Locations = locations;
            ViewBag.SelectedLocation = locationFilter ?? "All Locations";
            ViewBag.DirectorSearch = directorSearch;

            IQueryable<Chapter> chaptersQuery = _context.Chapters
                .Include(c => c.Directors) // ✅ FIXED: Ensure Directors are properly loaded
                .AsNoTracking();

            // Apply Location Filter
            if (!string.IsNullOrEmpty(locationFilter) && locationFilter != "All Locations")
            {
                chaptersQuery = chaptersQuery.Where(c => c.City == locationFilter);
            }

            //Apply Director Name Filter (Case-Insensitive, Partial Match)
            if (!string.IsNullOrEmpty(directorSearch))
            {
                string lowerSearch = directorSearch.ToLower();
                chaptersQuery = chaptersQuery.Where(c => c.Directors.Any(d => d.LastName.ToLower().Contains(lowerSearch)));
            }

            //Pagination logic
            int actualPageSize = pageSize.GetValueOrDefault(10);
            var pagedChapters = await PaginatedList<Chapter>.CreateAsync(chaptersQuery, page.GetValueOrDefault(1), actualPageSize);

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = actualPageSize;
            ViewData["TotalPages"] = pagedChapters.TotalPages;

            return View(pagedChapters);
        }

        // Create View
        //[Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Directors = new MultiSelectList(_context.Directors
                .Where(d => d.Status)
                .Select(d => new { d.ID, FullName = d.FirstName + " " + d.LastName }),
                "ID", "FullName");

            var availableDirectors = _context.Directors
                 .Where(d => d.Status) // Only include active directors
                 .Select(d => new SelectListItem
                 {
                     Value = d.ID.ToString(),
                     Text = d.FirstName + " " + d.LastName
                 })
                 .ToList();

            ViewBag.AvailableDirectors = availableDirectors;
            ViewBag.SelectedProvince = null;
            ViewData["ModalPopup"] = "hide";
            ViewData["returnURL"] = Url.Action("Index", "Chapter");
            return View();
        }

        // Create Chapter with Selected Directors
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Street,City,Province,ZipCode")] Chapter chapter, string SelectedDirectorIDs)
        {
            if (ModelState.IsValid)
            {
                var directorIDs = SelectedDirectorIDs?.Split(',').Select(int.Parse).ToList() ?? new List<int>();

                var selectedDirectors = await _context.Directors
                    .Where(d => directorIDs.Contains(d.ID))
                    .ToListAsync();

                chapter.Directors = selectedDirectors;

                _context.Add(chapter);
                await _context.SaveChangesAsync();

                ViewData["ModalPopupChap"] = "display";
                TempData["SuccessMsg"] = "Successfully created new chapter!";
            }
            else
            {
                TempData["ErrorMsg"] = "Error in creating chapter!";
            }

            ViewBag.Directors = new MultiSelectList(_context.Directors, "ID", "FullName", SelectedDirectorIDs);
            return View(chapter);
        }

        //Details Page
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Directors) // ✅ Ensure Directors are properly loaded
                .FirstOrDefaultAsync(m => m.ID == id);

            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // Edit View
        // GET: Chapter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Directors)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (chapter == null)
            {
                return NotFound();
            }

            ViewBag.Directors = new MultiSelectList(_context.Directors, "ID", "FullName", chapter.Directors.Select(d => d.ID));

            // Ensure returnURL is set for the "Back to Chapter" button
            ViewData["returnURL"] = Url.Action("Index", "Chapter");

            return View(chapter);
        }


        // Edit Chapter with Directors
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Street,City,Province,ZipCode")] Chapter chapter, string SelectedDirectorIDs)
        {
            if (id != chapter.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingChapter = await _context.Chapters
                        .Include(c => c.Directors)
                        .FirstOrDefaultAsync(c => c.ID == id);

                    if (existingChapter == null)
                    {
                        return NotFound();
                    }

                    // Update Chapter properties
                    existingChapter.Street = chapter.Street;
                    existingChapter.City = chapter.City;
                    existingChapter.Province = chapter.Province;
                    existingChapter.ZipCode = chapter.ZipCode;

                    // Update Directors
                    var directorIDs = SelectedDirectorIDs?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
                    existingChapter.Directors = await _context.Directors
                        .Where(d => directorIDs.Contains(d.ID))
                        .ToListAsync();

                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = $"Successfully updated Chapter: {chapter.City}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Chapters.Any(e => e.ID == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            TempData["ErrorMsg"] = "Error in updating Chapter.";
            ViewBag.Directors = new MultiSelectList(_context.Directors, "ID", "FullName");
            return View(chapter);
        }

        // Archive Chapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }

            chapter.Status = ChapterStatus.Archived;
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Chapter archived successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool ChapterExists(int id)
        {
            return _context.Chapters.Any(e => e.ID == id);
        }
    }
}
