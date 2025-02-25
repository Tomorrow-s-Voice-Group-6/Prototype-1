using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //// GET: Chapter
        //public async Task<IActionResult> Index()
        //{
        //    var tomorrowsVoiceContext = _context.Chapters.Include(c => c.Director);
        //    return View(await tomorrowsVoiceContext.ToListAsync());
        //}

        public async Task<IActionResult> Index(string locationFilter, int? page = 1, int? pageSize = 10)
        {
            // Fetch unique city locations for filtering
            var locations = await _context.Chapters
                                          .Where(c => !string.IsNullOrEmpty(c.City))
                                          .Select(c => c.City)
                                          .Distinct()
                                          .OrderBy(c => c)
                                          .ToListAsync();

            locations.Insert(0, "All Locations");
            ViewBag.Locations = locations;
            ViewBag.SelectedLocation = locationFilter ?? "All Locations";

            // Load Chapters with Directors
            IQueryable<Chapter> chaptersQuery = _context.Chapters
                .Include(c => c.Directors)
                .AsNoTracking();

            // Apply location filter
            if (!string.IsNullOrEmpty(locationFilter) && locationFilter != "All Locations")
            {
                chaptersQuery = chaptersQuery.Where(c => c.City == locationFilter);
            }

            // Pagination logic
            int actualPageSize = pageSize.GetValueOrDefault(10);
            var pagedChapters = await PaginatedList<Chapter>.CreateAsync(chaptersQuery, page.GetValueOrDefault(1), actualPageSize);

            // Pass pagination data to the view
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = actualPageSize;
            ViewData["TotalPages"] = pagedChapters.TotalPages;

            return View(pagedChapters);
        }



        // GET: Chapter/Create
        //public IActionResult Create()
        //{
        //    ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email");
        //    return View();
        //}


        // Update by Fernand Eddy - change Director email into fullName
        // GET: Chapter/Create
        [HttpGet("/Chapter/Create")]
        public IActionResult Create()
        {
            ViewBag.DirectorID = new SelectList(_context.Directors
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

            // Assign available directors to ViewBag for the view
            ViewBag.AvailableDirectors = availableDirectors;
            return View();
        }

        // POST: Chapter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Street,City,Province,ZipCode")] Chapter chapter, string SelectedDirectorIDs)
        {
            if (ModelState.IsValid)
            {
                var directorIDs = SelectedDirectorIDs?.Split(',').Select(id => int.Parse(id)).ToList() ?? new List<int>();

                var selectedDirectors = await _context.Directors
                    .Where(d => directorIDs.Contains(d.ID))
                    .ToListAsync();

                chapter.Directors = selectedDirectors;

                _context.Add(chapter);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Successfully created new chapter!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMsg"] = "Error in creating chapter! Please try again and ensure all fields are correctly completed.";
            }

            var availableDirectors = _context.Directors
                .Where(d => d.Status) 
                .Select(d => new SelectListItem
                {
                    Value = d.ID.ToString(),
                    Text = d.FirstName + " " + d.LastName
                })
                .ToList();

            ViewBag.AvailableDirectors = availableDirectors;

            return View(chapter);
        }



        // ✅ GET: Chapter/Edit/5
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






        // GET: Chapter/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var chapter = await _context.Chapters.FindAsync(id);
        //    if (chapter == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", chapter.DirectorID);
        //    return View(chapter);
        //}

        // POST: Chapter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // ✅ POST: Chapter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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



        // add by Fernand Eddy
        [HttpGet("/Chapter/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Directors)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (chapter == null)
            {
                return NotFound();
            }
            return View(chapter);
        }

        // GET: Chapter/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(chapter);
        }

        // POST: Chapter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChapterExists(int id)
        {
            return _context.Chapters.Any(e => e.ID == id);
        }
    }
}
