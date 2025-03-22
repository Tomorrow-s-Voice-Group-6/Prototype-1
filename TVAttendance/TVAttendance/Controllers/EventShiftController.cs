using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Data.Migrations;
using TVAttendance.Models;
using TVAttendance.Utilities;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class EventShiftController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public EventShiftController(TomorrowsVoiceContext context)
        {
            _context = context;
        }
        // GET: EventShift
        public async Task<IActionResult> Index(string? actionButton, DateOnly? fromDate, DateOnly? toDate, int? EventID, int? page = 1,
            string sortDirection = "asc", string sortField = "Location")
        {
            string[] sortOptions = new[] { "ShiftDate", "ShiftStart", "ShiftEnd" };
            if (EventID == null)
            {
                return NotFound("EventID is required.");
            }

            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Event");

            var shifts = _context.Shifts.Include(s => s.Event)
                .Include(s=>s.ShiftVolunteers)
                .ThenInclude(s=>s.Volunteer)
                .Where(e => e.EventID == EventID)
                .AsNoTracking();

            Event? thisEvent = await _context.Events
               .Include(s => s.Shifts)
               .AsNoTracking()
               .FirstOrDefaultAsync(s => s.ID == EventID);
            if (thisEvent == null)
            {
                return NotFound("The event does not exist.");
            }
            //Filters
            if (fromDate.HasValue)
                shifts = shifts.Where(d => d.ShiftDate >= fromDate);
            if (toDate.HasValue)
                shifts = shifts.Where(d => d.ShiftDate <= toDate.Value);

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
            if (sortField == "ShiftDate")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(p => p.ShiftDate)
                    : shifts.OrderByDescending(p => p.ShiftDate);
            }
            else if (sortField == "ShiftStart")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(v => v.ShiftStart)
                    : shifts.OrderByDescending(v => v.ShiftStart);
            }
            else if (sortField == "ShiftEnd")
            {
                shifts = sortDirection == "asc"
                    ? shifts.OrderBy(v => v.ShiftEnd)
                    : shifts.OrderByDescending(v => v.ShiftEnd);
            }
            #endregion

            //For indetifying ID in pages, update the ViewData in each method to the selected event
            ViewData["EventDetails"] = thisEvent;
            var test = ViewData["EventDetails"];
            //For titles
            ViewData["EventName"] = thisEvent.EventName;

            int pageSize = 3;
            int pageIndex = page ?? 1;
            int totalItems = await shifts.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedData = await PaginatedList<Shift>.CreateAsync(shifts, pageIndex, pageSize);

            // Pass pagination details to the view
            ViewData["CurrentPage"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(pagedData);
        }

        // GET: EventShift/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);

            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == shift.EventID);

            
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // GET: EventShift/Create
        public async Task<IActionResult> Create(int? id)
        {
            ViewData["ModalPopupShift"] = "hide";

            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == id);
               
            DateOnly date = DateOnly.FromDateTime(thisEvent.EventStart.Date);

            int shifts = _context.Shifts
                .Include(e => e.Event)
                .Where(e => e.EventID == id && e.ShiftDate == date)
                .Count();

            //ViewData's for display only
            ViewData["EventName"] = thisEvent.EventName;
            ViewData["EventStart"] = thisEvent.EventStart;
            ViewData["EventEnd"] = thisEvent.EventEnd;
            ViewData["EventRange"] = thisEvent.EventDate;
            ViewData["EventCap"] = thisEvent.VolunteerCapacity;
            ViewData["ShiftCount"] = shifts;

            //Create a new empty shift with an event id
            Shift shift = new Shift
            {
                EventID = thisEvent.ID
            };
            //new empty shift with an event id
            return View(shift);
        }

        // POST: EventShift/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,ShiftDate,ShiftStart,ShiftEnd")] Shift shift)
        {
            try
            {
                Event? thisEvent = await _context.Events
                    .Include(s => s.Shifts)
                    .FirstOrDefaultAsync(e => e.ID == shift.EventID);

                if (thisEvent == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    _context.Add(shift);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Successfully created new Shift";
                    ViewData["ModalPopupShift"] = "display";
                }
            }

            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.");
                ViewData["ModalPopupShift"] = "hide";

            }

            //Handles errors. When you remove this the viewdatas will go away because
            //Event? thisEvent is in the try block and will not persist
            Event? existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.ID == shift.EventID);

            if (existingEvent != null)
            {
                int shifts = _context.Shifts
                    .Include(e => e.Event)
                    .Where(e => e.EventID == existingEvent.ID && e.ShiftDate == shift.ShiftDate)
                    .Count();

                ViewData["EventName"] = existingEvent.EventName;
                ViewData["EventStart"] = existingEvent.EventStart;
                ViewData["EventEnd"] = existingEvent.EventEnd;
                ViewData["EventRange"] = existingEvent.EventDate;
                ViewData["EventCap"] = existingEvent.VolunteerCapacity;
                ViewData["ShiftCount"] = shifts;
            }
            return View(shift);
        }

        // GET: EventShift/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(e => e.Event)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (shift == null)
            {
                return NotFound();
            }
            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == shift.Event.ID);

            ViewData["EventRange"] = thisEvent.EventDate;
            ViewData["EventStart"] = thisEvent.EventStart;
            ViewData["EventEnd"] = thisEvent.EventEnd;
            ViewData["EventName"] = thisEvent.EventName;

            return View(shift);
        }

        // POST: EventShift/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift shift)
        {
            var shiftToUpdate = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.ID == id);
            
            if (shiftToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Shift>(shiftToUpdate, "", s => s.ShiftDate,
                s => s.ShiftStart, s => s.ShiftEnd))
            {
                try
                {
                    _context.Update(shiftToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Successfully updated the Shift!";
                    return RedirectToAction("Index", new { shiftToUpdate.EventID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shiftToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            //For ViewData's only
            Event? thisEvent = await _context.Events
               .Include(s => s.Shifts)
               .AsNoTracking()
               .FirstOrDefaultAsync(s => s.ID == shift.Event.ID);

            ViewData["EventRange"] = thisEvent.EventDate;
            ViewData["EventStart"] = thisEvent.EventStart;
            ViewData["EventEnd"] = thisEvent.EventEnd;
            ViewData["EventName"] = thisEvent.EventName;

            return View(shiftToUpdate);
        }

        // GET: EventShift/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // POST: EventShift/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Successfully removed Shift.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ShiftDateUpdate(int? id, string? updatedDate)
        {
            Event? thisEvent = await _context.Events
                .Include(s => s.Shifts)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID == id);

            //On shiftDate change
            DateOnly date;
            if (!updatedDate.IsNullOrEmpty())
            {
                date = DateOnly.Parse(updatedDate);
            }
            else
            {
                date = DateOnly.FromDateTime(thisEvent.EventStart.Date);
            }

            int shifts = _context.Shifts
                .Where(e => e.EventID == id && e.ShiftDate == date)
                .Count();

            return Json(new
            {
                success = true,
                shiftCount = shifts,
                eventCap = thisEvent.VolunteerCapacity
            });
        }
        public IActionResult ExportToExcel(int id)
        {
            var shifts = _context.Shifts
                .Include(s => s.Event)
                .Include(e => e.ShiftVolunteers)
                .ThenInclude(v=>v.Volunteer)
                .ToList();

            var data = new List<ExportShiftsVM>();

            foreach (var shift in shifts)
            {
                //for debugging (determing ShiftVolunteer values)
                var s = shift;
                string attended = shift.ShiftVolunteers.FirstOrDefault()?.NonAttendance.ToString() == null ? "Upcoming"
                    : shift.ShiftVolunteers.FirstOrDefault()?.NonAttendance.ToString();
                string notes = "";
                if (attended == "0")
                {
                    attended = "No";
                    notes = shift.ShiftVolunteers.FirstOrDefault()?.AttendanceReason.ToString();
                }
                else if (attended == "1")
                {
                    attended = "Yes";
                    shift.ShiftVolunteers.FirstOrDefault()?.Note.ToString();
                }
                data.Add(new ExportShiftsVM
                {
                    Name = shift.ShiftVolunteers.FirstOrDefault()?.Volunteer.FullName,
                    Attended = attended, //convert bool to string
                    startTime = shift.ShiftStart,
                    endTime = shift.ShiftEnd,
                    ShiftRange = shift.ShiftRange,
                    Notes = notes
                });
            }

            using (ExcelPackage excel = new ExcelPackage())
            {
                var workSheet = excel.Workbook.Worksheets.Add("Event-Details");

                //Titles and event details
                workSheet.Cells[1, 1].Value = "Report";
                workSheet.Cells[1, 5].Merge = true;
                workSheet.Cells[2, 1].Value = "Address";
                workSheet.Cells[2, 5].Merge = true;
                workSheet.Cells[3, 1].Value = "Date";
                workSheet.Cells[3, 5].Merge = true;
                workSheet.Cells[4, 1].Value = "Total Shifts:";
                workSheet.Cells[4, 5].Merge = true;

                var row = 6; //start on row 6 
                foreach (var record in data)
                {
                    workSheet.Cells[row, 1].Value = record.Name;
                    workSheet.Cells[row, 2].Value = record.Attended;
                    workSheet.Cells[row, 3].Value = record.TimeWorked;
                    workSheet.Cells[row, 4].Value = record.ShiftRange;
                    workSheet.Cells[row, 5].Value = record.Notes;
                    row++;
                }
                workSheet.Cells.AutoFitColumns();
                try
                {
                    Byte[] bytes = excel.GetAsByteArray();
                    string fileName = "Event-Details.xlsx";
                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    TempData["SuccessMsg"] = "Successfully built file. Will begin download shortly...";
                    return File(bytes, mimeType, fileName);
                }
                catch (Exception)
                {
                    TempData["ErrorMsg"] = "Could not build and download the file.";
                    return BadRequest("Could not build and download the file");
                }
            }
        }
        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ID == id);
        }
    }
}
