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

        public async Task<IActionResult> Index(string locationFilter, int pageSize = 10, int page = 1)
        {
            // Ensure unique city locations are fetched
            var locations = await _context.Chapters
                                          .Where(c => !string.IsNullOrEmpty(c.City))
                                          .Select(c => c.City)
                                          .Distinct()
                                          .ToListAsync();

            // Insert "All Locations" as the first option
            locations.Insert(0, "All Locations");
            ViewBag.Locations = locations;
            ViewBag.SelectedLocation = locationFilter ?? "All Locations";

            // Apply Include() before filtering
            IQueryable<Chapter> chaptersQuery = _context.Chapters.Include(c => c.Director);

            // Apply filter only if a valid location is selected
            if (!string.IsNullOrEmpty(locationFilter) && locationFilter != "All Locations")
            {
                chaptersQuery = chaptersQuery.Where(c => c.City == locationFilter);
            }

            // Pagination logic
            int totalItems = await chaptersQuery.CountAsync();
            var pagedData = await PaginatedList<Chapter>.CreateAsync(chaptersQuery.AsNoTracking(), page, pageSize);

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(pagedData);
        }




        // GET: Chapter/Create
        //public IActionResult Create()
        //{
        //    ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email");
        //    return View();
        //}


        // Update by Fernand Eddy - change Director email into fullName
        // GET: Chapter/Create
        // GET: Chapter/Create
        public IActionResult Create()
        {
            // Populate Director dropdown with names instead of emails
            ViewBag.DirectorID = new SelectList(_context.Directors
                .Select(d => new { d.ID, FullName = d.FirstName + " " + d.LastName }),
                "ID", "FullName");

            // Ensure "Choose a Province" appears in Province dropdown
            ViewBag.SelectedProvince = null;

            ViewData["returnURL"] = Url.Action("Index", "Chapter"); // ✅ Fix return URL
            return View();
        }

        // POST: Chapter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Street,City,Province,ZipCode,DirectorID")] Chapter chapter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chapter);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Successfully created new chapter!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMsg"] = "Error in creating chapter! Please try again and ensure all fields are correctly completed.";
            }

            // Populate Director dropdown with names instead of emails
            ViewBag.DirectorID = new SelectList(_context.Directors
                .Select(d => new { d.ID, FullName = d.FirstName + " " + d.LastName }),
                "ID", "FullName", chapter.DirectorID);

            return View(chapter);
        }



        public IActionResult Edit(int id)
        {
            var chapter = _context.Chapters.Find(id);
            if (chapter == null)
            {
                return NotFound();
            }

            ViewBag.DirectorID = new SelectList(_context.Directors
                .Select(d => new { d.ID, FullName = d.FirstName + " " + d.LastName }), "ID", "FullName", chapter.DirectorID);

            ViewBag.SelectedProvince = chapter.Province;
            ViewData["returnURL"] = Url.Action("Index", "Chapter"); 
            return View(chapter);
        }


        // GET: Chapter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", chapter.DirectorID);
            return View(chapter);
        }

        // POST: Chapter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,City,Address,DirectorID")] Chapter chapter)
        {
            if (id != chapter.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chapter);
                    TempData["SuccessMsg"] = $"Successfully updated {chapter.City}!";
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMsg"] = $"Error in updating Chapter: {chapter.City}";
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", chapter.DirectorID);
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
                .Include(c => c.Director)
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
