using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;

namespace TVAttendance.Controllers
{
    public class SingerController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public SingerController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Singer
        public async Task<IActionResult> Index(string? SearchString, int? ChapterID)
        {
            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            if (ChapterID.HasValue)
            {
                singers = singers.Where(c => c.ChapterID == ChapterID);
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                singers = singers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
            }

            PopulateLists();

            return View(await singers.ToListAsync());
        }

        // GET: Singer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        // GET: Singer/Create
        public IActionResult Create()
        {
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "City");
            return View();
        }

        // POST: Singer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,DOB,Address,Status,RegisterDate,EmergencyContactFirstName,EmergencyContactLastName,EmergencyContactPhone,ChapterID")] Singer singer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(singer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "City", singer.ChapterID);
            return View(singer);
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers.FindAsync(id);
            if (singer == null)
            {
                return NotFound();
            }
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "City", singer.ChapterID);
            return View(singer);
        }

        // POST: Singer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,LastName,DOB,Address,Status,RegisterDate,EmergencyContactFirstName,EmergencyContactLastName,EmergencyContactPhone,ChapterID")] Singer singer)
        {
            if (id != singer.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(singer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SingerExists(singer.ID))
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
            ViewData["ChapterID"] = new SelectList(_context.Chapters, "ID", "City", singer.ChapterID);
            return View(singer);
        }

        // GET: Singer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        // POST: Singer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var singer = await _context.Singers.FindAsync(id);
            if (singer != null)
            {
                _context.Singers.Remove(singer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SingerExists(int id)
        {
            return _context.Singers.Any(e => e.ID == id);
        }

        private SelectList ChapterList(int? selectedId)
        {
            return new SelectList(_context.Chapters
                .OrderBy(m => m.City), "ID", "City", selectedId);
        }

        private void PopulateLists(Singer singer = null)
        {
            ViewData["ChapterID"] = ChapterList(singer?.ChapterID);
        }
    }
}
