using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class SingerController : ElephantController
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
        string? actionButton,
        string? YoungDOB,
        string? OldestDOB,
        string? ToDate,
        string? FromDate,
        int? page,
        bool ActiveStatus = true,
        string sortDirection = "asc",
        string sortField = "Full Name"
        )
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            PopulateLists();
            
            singers = singers.Where(s => s.Status == ActiveStatus);

            string[] sortOptions = new[] { "Full Name", "E-Contact Phone", "Chapter" };

            // Filtering
            #region Filtering
            if (ChapterID.HasValue)
            {
                singers = singers.Where(c => c.ChapterID == ChapterID);
                numFilters++;
            }
            if (!SearchString.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numFilters++;
            }
            if (!OldestDOB.IsNullOrEmpty())
            {
                int age = int.Parse(OldestDOB);
                DateTime oldestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB > oldestAge);
                numFilters++;
            }
            if (!YoungDOB.IsNullOrEmpty())
            {
                int age = int.Parse(YoungDOB);
                DateTime youngestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB < youngestAge);
                numFilters++;
            }
            if (!FromDate.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.RegisterDate > DateTime.Parse(FromDate));
                numFilters++;
            }
            if (!ToDate.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.RegisterDate < DateTime.Parse(ToDate));
                numFilters++;
            }
            if (!ActiveStatus)
            {
                numFilters++;
            }
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            #endregion

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
            ViewData["ModalPopup"] = "hide";

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
                }

                ViewData["ModalPopup"] = "display";
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

        // GET: Singer/Edit/5
        public async Task<IActionResult> Restore(int? id)
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
                singerToUpdate.Status = true;
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
