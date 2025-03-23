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
        public async Task<IActionResult> Index(bool showArchived = false, int page = 1, int pageSize = 15, int? chapterId = null)
        {
            var query = _context.Directors.AsQueryable();

            if (showArchived)
            {
                query = query.Where(d => !d.Status); // Show only archived directors
            }
            else
            {
                query = query.Where(d => d.Status); // Show only active directors
            }

            // If a Chapter is selected, filter directors associated with that Chapter
            if (chapterId.HasValue)
            {
                query = query.Where(d => d.Chapters.Any(c => c.ID == chapterId.Value));
                ViewData["SelectedChapter"] = chapterId.Value;
            }
            else
            {
                ViewData["SelectedChapter"] = null;
            }

            var totalItems = await query.CountAsync();
            var directors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Apply Pagination
            var pagedDirectors = await PaginatedList<Director>.CreateAsync(query.AsNoTracking(), page, pageSize);

            // Pass pagination data to the view
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["ShowArchived"] = showArchived;

            // Load chapters for the dropdown.
            // Here I'm using "City" as the display text, but you can change it as needed.
            ViewData["ChapterList"] = new SelectList(_context.Chapters, "ID", "City");

            return View(pagedDirectors); // ✅ Now passing PaginatedList<Director>
        }





        // GET: Director/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Directors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (director == null)
            {
                return NotFound();
            }

            return View(director);
        }

        // GET: Director/Create
        public IActionResult Create()
        {
            ViewData["ModalPopupdir"] = "hide";
            Director director = new Director();
            PopulateLists();
            return View();
        }

        // POST: Director/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Email,Phone,Status")] Director director)
        {
            try
            {
                
                if (ModelState.IsValid)
                {
                    _context.Add(director);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = $"Successfully created {director.FullName}!";
                    
                }
                ViewData["ModalPopupdir"] = "display";

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

            PopulateLists(director);
            return View(director);
        }

        // GET: Director/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Directors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ID == id);
            if (director == null)
            {
                return NotFound();
            }

            PopulateLists();
            return View(director);
        }

        // POST: Director/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            var directorToUpdate = await _context.Directors
                .FirstOrDefaultAsync(d => d.ID == id);


            if (directorToUpdate == null)
            {
                return NotFound();
            }

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
                    TempData["ErrorMsg"] = "Error in updating a Director. Please try again or contact the administrator.";
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



        private SelectList DirectorList(int? selectedId)
        {
            return new SelectList(_context.Directors
                .OrderBy(d => d.FullName),"ID","FullName",selectedId);
        }

        private void PopulateLists(Director director = null)
        {
            ViewData["FullName"] = DirectorList(director?.ID);
        }

        private bool DirectorExists(int id)
        {
            return _context.Directors.Any(e => e.ID == id);
        }
    }
}
