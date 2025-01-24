using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public async Task<IActionResult> Index(string? SearchString, int? ChapterID, bool? ActiveStatus,
            string? actionButton, string sortDirection = "asc", string sortField = "Status")
        {
            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            string[] sortOptions = new[] { "Full Name", "Status", "E-Contact Phone", "Chapter" };

            if (ChapterID.HasValue)
            {
                singers = singers.Where(c => c.ChapterID == ChapterID);
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                singers = singers.Where(s => s.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (ActiveStatus.HasValue)
            {
                singers = singers.Where(s => s.Status == ActiveStatus.GetValueOrDefault());
            }

            //sorting options
            #region sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }
            if (sortField == "Full Name")
            {
                if (sortDirection == "asc")
                {
                    singers = singers
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    singers = singers
                        .OrderByDescending(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
            }
            if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    singers = singers
                        .OrderByDescending(p => p.Status)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    singers = singers
                        .OrderBy(p => p.Status)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
            }
            if (sortField == "Chapter")
            {
                if (sortDirection == "asc")
                {
                    singers = singers
                        .OrderBy(p => p.Chapter.City)
                        .ThenByDescending(p => p.Status)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    singers = singers
                        .OrderByDescending(p => p.Chapter.City)
                        .ThenBy(p => p.Status)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
            }
            #endregion
            
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

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
            PopulateLists();
            return View();
        }

        // POST: Singer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,DOB," +
            "Address,Status,RegisterDate," +
            "EmergencyContactFirstName,EmergencyContactLastName," +
            "EmergencyContactPhone,ChapterID")] Singer singer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(singer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch(DbUpdateException ex)
            {
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Clients.DOB"))
                {
                    ModelState.AddModelError("SingerCompositeKey", "Unable to save changes. Remember, " +
                        "you cannot have duplicate Singers.  First name, last name, and DOB must be Unique.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            

            PopulateLists(singer);
            return View(singer);
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            PopulateLists();
            return View(singer);
        }

        // POST: Singer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var singerToUpdate = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Singer>(singerToUpdate, "",
                s=>s.FirstName, s=>s.LastName, s => s.DOB, s => s.Address,
                s => s.RegisterDate, s => s.EmergencyContactFirstName, s => s.EmergencyContactLastName,
                s => s.EmergencyContactPhone, s => s.ChapterID))
            {
                try
                {
                    _context.Update(singerToUpdate);
                    await _context.SaveChangesAsync();
                    if (ModelState.IsValid)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SingerExists(singerToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }

            
            PopulateLists(singerToUpdate);
            return View(singerToUpdate);
        }

        //Delete action is not in use for Singers
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
