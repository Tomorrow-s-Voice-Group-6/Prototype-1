using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class VolunteerController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Volunteer
        public async Task<IActionResult> Index(int? page, string? actionButton, string? FullName, DateTime? dobFromDate, DateTime? dobToDate, 
            DateTime? regFromDate, DateTime? regToDate, string sortDirection = "asc", string sortField = "Date")
        {
            int filters = 0;

            string[] sortOptions = { "FullName", "DOB", "RegisterDate" };

            var volunteers = _context.Volunteers
                .Include(v => v.ShiftVolunteers)
                .ThenInclude(s=>s.Shift)
                .AsNoTracking();

            #region Filtering
            if (!String.IsNullOrEmpty(FullName))
            {
                FullName = FullName.Trim();
                int lastSpaceIndex = FullName.LastIndexOf(' ');

                string filterFirst = "";
                string filterLast = "";
                if (lastSpaceIndex != -1)
                {
                    filterFirst = FullName.Substring(0, lastSpaceIndex);
                    filterLast = FullName.Substring(lastSpaceIndex + 1);
                }
                if (!String.IsNullOrEmpty(filterLast) && !String.IsNullOrEmpty(filterFirst))
                {
                    volunteers = volunteers.Where(v => v.LastName.ToUpper().Contains(filterLast.Trim().ToUpper()) &&
                    v.FirstName.ToUpper().Contains(filterFirst.ToUpper()));
                } else
                {
                    volunteers = volunteers.Where(v => v.LastName.ToUpper().Contains(FullName.ToUpper()) ||
                    v.FirstName.ToUpper().Contains(FullName.ToUpper()));
                }
                
                filters++;
            }
            if (dobFromDate.HasValue)
            {
                volunteers = volunteers.Where(v => v.DOB >= dobFromDate);
                filters++;
            }
            if (dobToDate.HasValue && dobToDate.Value != DateTime.Today)
            {
                volunteers = volunteers.Where(d => d.DOB <= dobToDate.Value);
                filters++;
            }
            if (regFromDate.HasValue)
            {
                volunteers = volunteers.Where(v => v.RegisterDate >= regFromDate);
                filters++;
            }
            if (regToDate.HasValue && regToDate.Value != DateTime.Today)
            {
                volunteers = volunteers.Where(d => d.RegisterDate <= regToDate.Value);
                filters++;
            }
            if (filters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({filters} Filter{(filters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            #endregion

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
            if (sortField == "FullName")
            {
                volunteers = sortDirection == "asc"
                    ? volunteers.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : volunteers.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName);
            }
            else if (sortField == "DOB")
            {
                volunteers = sortDirection == "asc"
                    ? volunteers.OrderBy(v => v.DOB)
                    : volunteers.OrderByDescending(v => v.DOB);
            }
            else if (sortField == "RegisterDate")
            {
                volunteers = sortDirection == "asc"
                    ? volunteers.OrderBy(v => v.RegisterDate)
                    : volunteers.OrderByDescending(v => v.RegisterDate);
            }
            #endregion
            // Pagination
            var totalItems = await volunteers.CountAsync();
            int pageSize = 10;
            var pagedData = await PaginatedList<Volunteer>.CreateAsync(volunteers.AsNoTracking(), page ?? 1, pageSize);

            // ViewData for paging and others
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
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
                .Include(v => v.ShiftVolunteers)
                .ThenInclude(s => s.Shift)
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
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Volunteer");
            ViewData["ModalPopup"] = "hide";
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
                if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.Phone, @"^\d{3}-\d{3}-\d{4}$"))
                {
                    ModelState.AddModelError("Phone", "Phone number must be in the format 000-000-0000.");
                    return View(volunteer);
                }

                if (ModelState.IsValid)
                {
                    _context.Add(volunteer);
                    await _context.SaveChangesAsync();
                    //TempData["SuccessMsg"] = "Successfully created a new Volunteer!";
                    ViewData["ModalPopup"] = "display";
                }
                
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMsg"] = "Error in creating a new Volunteer. Please ensure all fields are completed and try again.";
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("DOB"))
                {
                    ModelState.AddModelError("", "Unable to save changes." +
                        "  You cannot have duplicate Volunteers.  First name, last name, and Date of Birth must be Unique.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
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
                .Include(v => v.ShiftVolunteers)
                .ThenInclude(s => s.Shift)
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

                    if (!System.Text.RegularExpressions.Regex.IsMatch(volunteer.Phone, @"^\d{3}-\d{3}-\d{4}$"))
                    {
                        ModelState.AddModelError("Phone", "Phone number must be in the format 000-000-0000.");
                        return View(volunteer);
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Successfully updated the Volunteer!";
                    return RedirectToAction("Index", "VolunteerShift", new { VolunteerID = volunteer.ID });
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
