using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Volunteer
        public async Task<IActionResult> Index(int? page=1)
        {
            var volunteers = _context.Volunteers
                .Include(v => v.VolunteerEvents)
                .AsNoTracking();

            //Paging
            var totalItems = await volunteers.CountAsync();
            int pageSize = 10;
            var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers, page ?? 1, pageSize);

            //ViewData's for paging and others
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            //ViewData["sortField"] = sortField;
            //ViewData["sortDirection"] = sortDirection;

            return View(pagedData);
        }

        // GET: Volunteer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers
                .Include(v => v.VolunteerEvents)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (volunteer == null)
            {
                return NotFound();
            }
            return View(volunteer);
        }

        // GET: Volunteer/Create
        public IActionResult Create()
        {
            Volunteer v = new Volunteer(); //New empty volunteer for DDL's
            return View(v);
        }

        // POST: Volunteer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Phone,Email,DOB,RegisterDate,ChapterID")] Volunteer volunteer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(volunteer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Successfully created a new Volunteer!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMsg"] = "Error in creating a new Volunteer. Please ensure all fields are completed and try again.";
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts." +
                    " Try again, and if the problem persists, see your system administrator.");
            }
            return View(volunteer);
        }

        // GET: Volunteer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers
                .Include(e => e.VolunteerEvents)
                .FirstOrDefaultAsync(v => v.ID == id);
            if (volunteer == null)
            {
                return NotFound();
            }
            return View(volunteer);
        }

        // POST: Volunteer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,LastName,Phone,Email,DOB,RegisterDate")] Volunteer volunteer)
        {
            var volToUpdate = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.ID == id);

            if (volToUpdate == null)
                return NotFound();

            if (await TryUpdateModelAsync<Volunteer>(volToUpdate, "", 
                v=>v.FirstName, v=>v.LastName, v=>v.Phone, v=>v.Email,
                v=>v.DOB, v=>v.RegisterDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Successfully updated the Volunteer!";
                    return RedirectToAction("Details", new { volToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VolunteerExists(volToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException) 
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, " +
                       "and if the problem persists see your system administrator.");
                }
            }
            else
            {
                TempData["ErrorMsg"] = "Unknown error in editing the volunteer. Please try again";
            }
            return View(volToUpdate);
        }

        //Not needing archiving right now, but here are some placeholder methods
        //public async Task<IActionResult> Archive(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var volunteer = await _context.Volunteers
        //        .Include(v => v.VolunteerEvents)
        //        .FirstOrDefaultAsync(v => v.ID == id);

        //    if (volunteer == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(volunteer);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Archive(int id)
        //{
        //    var volToArchive = await _context.Volunteers
        //        .Include(v => v.VolunteerEvents)
        //        .FirstOrDefaultAsync(v => v.ID == id);

        //    if (volToArchive == null)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        volToArchive.Status = false;
        //        _context.Update(volToArchive);
        //        await _context.SaveChangesAsync();

        //        var returnUrl = ViewData["returnURL"]?.ToString();
        //        if (string.IsNullOrEmpty(returnUrl))
        //        {
        //            return RedirectToAction(nameof(Index));
        //        }
        //        return Redirect(returnUrl);

        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!VolunteerExists(volToArchive.ID))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}
        private bool VolunteerExists(int id)
        {
            return _context.Volunteers.Any(e => e.ID == id);
        }
    }
}
