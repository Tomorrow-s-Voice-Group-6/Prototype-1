using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
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
        public async Task<IActionResult> Index(int? page = 1)
        {
            var volunteers = _context.Volunteers
                .Include(v => v.VolunteerEvents)
                .AsNoTracking();

            // Paging
            var totalItems = await volunteers.CountAsync();
            int pageSize = 10;
            var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers, page ?? 1, pageSize);

            // ViewData for paging and others
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);

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
            Volunteer v = new Volunteer(); // New empty volunteer for DDL's
            return View(v);
        }

        // POST: Volunteer/Create
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,LastName,Phone,Email,DOB,RegisterDate")] Volunteer volunteer)
        {
            var volToUpdate = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.ID == id);

            if (volToUpdate == null)
                return NotFound();

            if (await TryUpdateModelAsync<Volunteer>(volToUpdate, "",
                v => v.FirstName, v => v.LastName, v => v.Phone, v => v.Email,
                v => v.DOB, v => v.RegisterDate))
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

        // Excel Export for Volunteer Data
        public async Task<IActionResult> ExportVolunteers()
        {
            var volunteers = await _context.Volunteers.ToListAsync();  // Retrieve volunteer data

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Volunteers");

                // Add headers to the Excel sheet
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Phone";
                worksheet.Cells[1, 4].Value = "Date of Birth";
                worksheet.Cells[1, 5].Value = "Register Date";

                // Add data rows
                int row = 2;
                foreach (var volunteer in volunteers)
                {
                    worksheet.Cells[row, 1].Value = volunteer.FullName;
                    worksheet.Cells[row, 2].Value = volunteer.Email;
                    worksheet.Cells[row, 3].Value = volunteer.Phone;
                    worksheet.Cells[row, 4].Value = volunteer.DOBFormatted;
                    worksheet.Cells[row, 5].Value = volunteer.RegisterFormatted;
                    row++;
                }

                // Generate the Excel file as a byte array
                var fileBytes = package.GetAsByteArray();

                // Return the Excel file as a downloadable file
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Volunteers.xlsx");
            }
        }

        // Check if a volunteer exists
        private bool VolunteerExists(int id)
        {
            return _context.Volunteers.Any(e => e.ID == id);
        }
    }
}
