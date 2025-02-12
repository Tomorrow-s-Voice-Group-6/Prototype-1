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

namespace TVAttendance.Controllers
{
    public class ChapterController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public ChapterController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Chapter
        public async Task<IActionResult> Index()
        {
            var tomorrowsVoiceContext = _context.Chapters.Include(c => c.Director);
            return View(await tomorrowsVoiceContext.ToListAsync());
        }

        // GET: Chapter/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Chapter/Create
        public IActionResult Create()
        {
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email");
            return View();
        }

        // POST: Chapter/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,City,Address,DirectorID")] Chapter chapter)
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
                TempData["ErrorMsg"] = "Error in creating chapter! Please try again and ensure all " +
                    "fields are correctly completed.";
            }
            ViewData["DirectorID"] = new SelectList(_context.Directors, "ID", "Email", chapter.DirectorID);
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
