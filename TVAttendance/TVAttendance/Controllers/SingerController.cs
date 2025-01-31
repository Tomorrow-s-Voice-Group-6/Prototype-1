using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

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
        public async Task<IActionResult> Index(
        string? SearchString,
        int? ChapterID,
        bool? ActiveStatus,
        string? actionButton,
        int? page,
        string sortDirection = "desc",
        string sortField = "Status"
        )
        {
            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            PopulateLists();

            string[] sortOptions = new[] { "Full Name", "Status", "E-Contact Phone", "Chapter" };

            // Filtering
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

            // Sorting
            #region Sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

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
                singers = sortDirection == "asc"
                    ? singers.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName);
            }
            else if (sortField == "Status")
            {
                singers = sortDirection == "asc"
                    ? singers.OrderBy(p => p.Status).ThenBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderByDescending(p => p.Status).ThenBy(p => p.FirstName).ThenBy(p => p.LastName);
            }
            else if (sortField == "Chapter")
            {
                singers = sortDirection == "asc"
                    ? singers.OrderByDescending(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderBy(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName);
            }
            #endregion

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Pagination
            int pageSize = 10;
            var pagedData = await PaginatedList<Singer>.CreateAsync(singers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
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
            "EmergencyContactPhone,ChapterID")] Singer singer, string singerCreateAdd)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(singer);
                    await _context.SaveChangesAsync();

                    if (singerCreateAdd.Contains("Add"))
                    {
                        return RedirectToAction(nameof(Create));
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch(DbUpdateException ex)
            {
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Singers.DOB"))
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
                s => s.EmergencyContactPhone, s => s.ChapterID, s=>s.Status))
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

        // GET: Singer/Edit/5
        public async Task<IActionResult> Archive(int? id)
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

            return View(singer);
        }

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var singerToUpdate = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            try
            {
                singerToUpdate.Status = false;
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
