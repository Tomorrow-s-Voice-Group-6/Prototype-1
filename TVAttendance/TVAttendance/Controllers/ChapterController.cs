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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Supervisor, Admin")]
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
        [Authorize(Roles = "Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Supervisor, Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Fetch only active directors from the database
            var availableDirectors = _context.Directors
                .Where(d => d.Status) // Only include active directors
                .Select(d => new SelectListItem
                {
                    Value = d.ID.ToString(),
                    Text = d.FirstName + " " + d.LastName
                })
                .ToList();

            // Fetch the selected directors for the chapter (if applicable)
            var existingChapter = _context.Chapters
                .Include(c => c.Directors)
                .FirstOrDefault(c => c.ID == id);

            if (existingChapter == null)
            {
                return NotFound();
            }

            // Get the selected directors for this chapter
            var selectedDirectors = existingChapter.Directors?
                .Select(d => new SelectListItem
                {
                    Value = d.ID.ToString(),
                    Text = d.FirstName + " " + d.LastName
                })
                .ToList() ?? new List<SelectListItem>();

            ViewBag.SelectedDirectors = selectedDirectors;

            // Exclude selected directors from the available directors list
            availableDirectors = availableDirectors
                .Where(d => !selectedDirectors.Any(sd => sd.Value == d.Value))
                .ToList();

            // Add directors to ViewBag
            ViewBag.AvailableDirectors = availableDirectors;
            ViewBag.SelectedDirectors = selectedDirectors;

            return View(existingChapter);
        }


        // Edit Chapter with Directors
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public IActionResult Edit(int id, Chapter chapter, string selectedDirectorIDs)
        {
            if (id != chapter.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Find the chapter in the database
                    var existingChapter = _context.Chapters
                        .Include(c => c.Directors)
                        .FirstOrDefault(c => c.ID == id);

                    if (existingChapter == null)
                    {
                        return NotFound();
                    }

                    // Split the selected director IDs and get the corresponding directors
                    var directorIds = selectedDirectorIDs.Split(',').Select(int.Parse).ToList();
                    var directors = _context.Directors
                        .Where(d => directorIds.Contains(d.ID))
                        .ToList();

                    // Update the chapter's properties
                    existingChapter.City = chapter.City;
                    existingChapter.Street = chapter.Street;
                    existingChapter.Province = chapter.Province;
                    existingChapter.ZipCode = chapter.ZipCode;

                    // Update the directors
                    existingChapter.Directors = directors;

                    // Save the changes to the database
                    _context.Update(existingChapter);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapter.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(chapter);
        }

        // Archive Chapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
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
