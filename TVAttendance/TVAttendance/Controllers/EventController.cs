using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting.Internal;
using OfficeOpenXml;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;
using static NuGet.Packaging.PackagingConstants;
using TVAttendance.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace TVAttendance.Controllers
{
    public class EventController : ElephantController 
    {

        private readonly IMyEmailSender _emailSender;
        private readonly TomorrowsVoiceContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public EventController(TomorrowsVoiceContext context, IWebHostEnvironment hostingEnvironment, IMyEmailSender emailSender)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _emailSender = emailSender;
        }

        // GET: Event
        [Authorize]
        public async Task<IActionResult> Index(
            string? actionButton,
            string? EventName,
            string? CityName,
            DateTime? fromDate,
            DateTime? toDate,
            int? page,
            int? pageSizeID,
            IFormFile? file,
            string sortDirection = "asc",
            bool ActiveStatus = true,
            string sortField = "EventName")
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

            string[] sortOptions = { "EventName", "EventStart", "EventEnd" };
            
            var events = _context.Events
            .Include(e => e.Shifts)
            .ThenInclude(e => e.ShiftVolunteers)
            .AsNoTracking();

            events = events.Where(s => s.EventOpen == ActiveStatus);

            if(actionButton == "Upload")
            {
                if (file == null)
                {
                    TempData["ErrorMsg"] = "No file provided.";
                }
                else
                {
                    UploadExcel(file);
                }
            }

            #region Filter
            //filters
            if (!String.IsNullOrEmpty(CityName))
            {
                events = events.Where(s => s.EventCity.ToUpper().Contains(CityName.ToUpper()));
                numFilters++;
            }
            if (!String.IsNullOrEmpty(EventName))
            {
                events = events.Where(s => s.EventName.ToUpper().Contains(EventName.ToUpper()));
                numFilters++;
            }
            if (fromDate.HasValue && fromDate != new DateTime(2022, 1, 1))
            {
                events = events.Where(d => d.EventStart >= fromDate.Value);
                numFilters++;
            }
            if (toDate.HasValue && toDate.Value != DateTime.Today)
            {
                events = events.Where(d => d.EventEnd <= toDate.Value);
                numFilters++;
            }
            if (!ActiveStatus)
            {
                numFilters++;
            }

            // Update UI for filters
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            else
            {
                ViewData["numFilters"] = "";
                ViewData["ShowFilter"] = "";
            }
            #endregion

            #region Sorting
            //sorting 
            // currently disabled in index 
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

            if (sortField == "EventName") 
            {
                events = sortDirection == "asc"
                  ? events.OrderBy(e => e.EventName)
                  : events.OrderByDescending(e => e.EventName);
            }
            else if (sortField == "EventStart") 
            {
                if (sortDirection == "asc")
                {
                    events = events
                        .OrderByDescending(d => d.EventStart);
                }
                else
                {
                    events = events
                        .OrderBy(d => d.EventStart);
                }
            }
            else if (sortField == "EventEnd")
            {
                if (sortDirection == "asc")
                {
                    events = events
                        .OrderByDescending(d => d.EventEnd);
                }
                else
                {
                    events = events
                        .OrderBy(d => d.EventEnd);
                }
            }
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            #endregion

            // Pagination
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Event/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tvEvent = await _context.Events
            .Include(e => e.Shifts)
            .ThenInclude(e => e.ShiftVolunteers)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (tvEvent == null)
            {
                return NotFound();
            }

            return View(tvEvent);
        }

        // GET: Event/Create
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public IActionResult Create()
        {
            ViewData["ModalPopupEvent"] = "hide";
            Event tvEvent = new Event();
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,EventName,EventStreet,EventCity,EventPostalCode,EventProvince,EventStart,EventEnd")] Event tvEvent)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    tvEvent.EventOpen = true;
                    _context.Add(tvEvent);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = $"Successfully created {tvEvent.EventName}!";
                }
                ViewData["EventID"] = tvEvent.ID;
                ViewData["ModalPopupEvent"] = "display";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMsg"] = "Error in creating a Event. Please try again or contact the administrator.";
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Events.Name") && message.Contains("Events.EventStreet"))
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts." +
                       " Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(tvEvent);
        }

        // GET: Event/Edit/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tvEvent = await _context.Events
                .Include(e => e.Shifts)
                .ThenInclude(e => e.ShiftVolunteers)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (tvEvent == null)
            {
                return NotFound();
            }
            return View(tvEvent);
        }

        // POST: Event/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EventName,EventStreet,EventCity,EventPostalCode,EventProvince,EventStart,EventEnd, EventOpen")] Event @event)
        {
            if (id != @event.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.ID))
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
            return View(@event);
        }

        public async Task<IActionResult> Close(int id)
        {
            Event? thisEvent = await _context.Events
                .FirstOrDefaultAsync(e=>e.ID == id);

            var shifts = _context.Shifts
                .Include(s => s.ShiftVolunteers)
                .ThenInclude(v => v.Volunteer)
                .Where(s=>s.EventID == id)
                .AsNoTracking();

            var shiftsAvai = shifts.Where(a => a.ShiftVolunteers.Count == 0);

            var shiftsOcc = shifts.Where(a => a.ShiftVolunteers.Count > 0 &&
                                   a.ShiftStart.CompareTo(DateTime.Now) > 0);

            CloseEmail(shiftsOcc, thisEvent);

            if (thisEvent == null)
            {
                return NotFound();
            }

            thisEvent.EventOpen = false;

            try
            {
                _context.RemoveRange(shiftsAvai);
                _context.Update(thisEvent);
                _context.SaveChanges();
            }
            catch
            {

            }

            return RedirectToAction("Index", "EventShift", new { EventID = thisEvent.ID});
        }

        //// GET: Event/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @event = await _context.Events
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (@event == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(@event);
        //}

        //// POST: Event/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var @event = await _context.Events.FindAsync(id);
        //    if (@event != null)
        //    {
        //        _context.Events.Remove(@event);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        private async void CloseEmail(IEnumerable<Shift> shifts, Event thisEvent)
        {
            List<EmailAddress> emailList = new List<EmailAddress>();

            foreach (Shift s in shifts)
            {
                EmailAddress email = new EmailAddress
                {
                    Name = s.ShiftVolunteers.FirstOrDefault().Volunteer.FullName,
                    Address = s.ShiftVolunteers.FirstOrDefault().Volunteer.Email
                };

                emailList.Add(email);
            }

            if (emailList.Count > 0)
            {
                var msg = new EmailMessage()
                {
                    ToAddresses = emailList,
                    Subject = $"{thisEvent.EventName}",
                    Content = $"The {thisEvent.EventName} in {thisEvent.EventCity} on {thisEvent.EventStartDate} has been closed.  Your shift has been cancelled."
                };
                await _emailSender.SendToManyAsync(msg);
            }
        }

        // DOWNLOAD EXCEL TEMPLATE
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public IActionResult DownloadTemplate()
        {
            var fileName = "EventTemplate.xlsx";
            var templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "templates");
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }
            var filePath = Path.Combine(templatePath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Template file not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // UPLOAD EXCEL FILE
        [HttpPost]
        [Authorize(Roles = "Director, Supervisor, Admin")]
        public async void UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "No file selected!";
            }

            var events = new List<Event>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 3; row <= rowCount; row++) // Start from row 2 (Skipping headers)
                    {
                        var eventName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var eventStreet = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var eventCity = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var eventPostalCode = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var eventProvinceStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var eventStartStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        var eventEndStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(eventStreet) ||
                            string.IsNullOrEmpty(eventCity) || string.IsNullOrEmpty(eventPostalCode) ||
                            string.IsNullOrEmpty(eventProvinceStr) || string.IsNullOrEmpty(eventStartStr) ||
                            string.IsNullOrEmpty(eventEndStr))
                        {
                            TempData["Message"] = $"Invalid data in row {row}";
                        }

                        if (!Enum.TryParse(eventProvinceStr, out Province eventProvince))
                        {
                            TempData["Message"] = $"Invalid province in row {row}: {eventProvinceStr} Please Ensure Proper Capitalization Without Spaces!";
                        }

                        if (!DateTime.TryParse(eventStartStr, out DateTime eventStart) ||
                            !DateTime.TryParse(eventEndStr, out DateTime eventEnd))
                        {
                            TempData["Message"] = $"Invalid date format in row {row}";
                        }
                        else
                        {
                            events.Add(new Event
                            {
                                EventName = eventName,
                                EventStreet = eventStreet,
                                EventCity = eventCity,
                                EventPostalCode = eventPostalCode,
                                EventProvince = eventProvince,
                                EventStart = eventStart,
                                EventEnd = eventEnd
                            });
                        }
                    }
                }
            }

            _context.Events.AddRange(events);
            _context.SaveChanges();

            TempData["Message"] = "File uploaded and processed successfully!";
        }
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.ID == id);
        }
    }
}
