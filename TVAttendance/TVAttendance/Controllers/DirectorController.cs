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
    public class DirectorController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public DirectorController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Director
        public async Task<IActionResult> Index(int page = 1, int pageSize = 15)
        {

            var totalItems = await _context.Directors.CountAsync();


            var directors = await _context.Directors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(directors);
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
            Director director = new Director();
            PopulateLists();
            return View();
        }

        // POST: Director/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,HireDate,Email,Phone,Status")] Director director)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(director);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
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
                d => d.FirstName, d => d.LastName, d => d.DOB, d => d.HireDate, d => d.Address,
                d => d.Email, d => d.Phone, d => d.Status))
            { 
                try
                {
                    _context.Update(directorToUpdate);
                    await _context.SaveChangesAsync();
                    if (ModelState.IsValid)
                    {
                        return RedirectToAction(nameof(Index));
                    }
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

        // GET: Director/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var director = await _context.Directors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (director == null)
            {
                return NotFound();
            }

            return View(director);
        }

        // POST: Director/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var director = await _context.Directors.FindAsync(id);
            if (director != null)
            {
                _context.Directors.Remove(director);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
