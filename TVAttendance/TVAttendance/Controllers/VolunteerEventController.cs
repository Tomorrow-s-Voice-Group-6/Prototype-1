using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class VolunteerEventController : Controller
    {
        private readonly TomorrowsVoiceContext _context;

        public VolunteerEventController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: VolunteerEvent
        public async Task<IActionResult> Index(int? selMonth, int? selYear,
            bool findClosestUpcoming = false, bool findClosestPast = false)
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int year = selYear ?? DateTime.Now.Year;
            int month = selMonth ?? DateTime.Now.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            if (findClosestUpcoming)
            {
                //If find closest button is clicked
                //Find the volunteer events ahead of today
                //Order by start date so the closest one to today is first
                //Select first date
                var closestEvent = await _context.VolunteerEvents
                    .Include(e => e.Event)
                    .Include(e => e.Volunteer)
                    .Where(e => e.Event.EventStart >= DateTime.Today)
                    .OrderBy(e => e.Event.EventStart)
                    .FirstOrDefaultAsync();

                if(closestEvent != null)
                {
                    year = closestEvent.Event.EventStart.Year;
                    month = closestEvent.Event.EventStart.Month;

                    //Since calendar is already created with the findClosest boolean = false,
                    //We can return to the same index view, except with the filter being
                    //The closest event to today's month and year
                    return RedirectToAction("Index", new {selMonth = month, selYear = year});
                }
                TempData["WarningMsg"] = "Warning: There are no upcoming events!";
            }
            else if (findClosestPast)
            {
                var closestEvent = await _context.VolunteerEvents
                    .Include(e => e.Event)
                    .Include(e => e.Volunteer)
                    .Where(e => e.Event.EventStart <= DateTime.Today)
                    .OrderByDescending(e => e.Event.EventStart)
                    .FirstOrDefaultAsync();

                if (closestEvent != null)
                {
                    year = closestEvent.Event.EventStart.Year;
                    month = closestEvent.Event.EventStart.Month;

                    return RedirectToAction("Index", new { selMonth = month, selYear = year });
                }
            }
            //Events for the month
            var events = await _context.VolunteerEvents
                .Include(e => e.Event)
                .Include(e => e.Volunteer)
                .Where(e => e.Event.EventStart.Year == year &&
                    e.Event.EventStart.Month == month)
                .ToListAsync();

            //All total events (Debugging)
            var allEvents = await _context.VolunteerEvents
                .Include(e => e.Event)
                .Include(e => e.Volunteer)
                .OrderBy(e => e.Event.EventStart.Month)
                .ThenBy(e => e.Event.EventStart.Year)
                .ToListAsync();

            PopulateDDLs(year);
            //Create a list  Calendar 
            List<CalendarVM> calendar = new List<CalendarVM>();

            //Create per month
            for (int i = 1; i <= daysInMonth; i++)
            {
                //Curent date using I for the days
                DateTime current = new DateTime(year, month, i);

                var dailyEvents = events
                    //Filter for events that are happening on the day
                    .Where(e => e.Event.EventStart.Date == current)
                    .GroupBy(e => e.Event.ID)
                    .Select(el => new EventListVM
                    {
                        ID = el.Key,
                        VolunteerLst = el.Select(v => new VolunteerVM
                        {
                            ID = v.VolunteerID,
                            Name = $"{v.Volunteer.FirstName} {v.Volunteer.LastName}",
                            ShiftStart = v.ShiftStart,
                            ShiftEnd = v.ShiftEnd,
                        }).ToList()
                    }).ToList();

                calendar.Add(new CalendarVM
                {
                    Date = current,
                    EventLst = dailyEvents
                });
            }
            
            return View(calendar);
        }

        // GET: VolunteerEvent/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerEvent = await _context.VolunteerEvents
                .Include(v => v.Event)
                .Include(v => v.Volunteer)
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (volunteerEvent == null)
            {
                return NotFound();
            }

            return View(volunteerEvent);
        }

        //Get: Event Details for modal popup
        public async Task<JsonResult> GetEventDetails(int id)
        {
            var eventDetails = await _context.Events
                .Include(v => v.VolunteerEvents).ThenInclude(v => v.Volunteer)
                .FirstOrDefaultAsync(e => e.ID == id);
            if (eventDetails != null)
            {
                var eventData = new //load data to be displayed in _CalanderPartialView.cshtml javascript
                { //Must be lowercase because of json and js being strict
                    name = eventDetails.EventName,
                    description = eventDetails.EventAddress,
                    timing = eventDetails.EventTime,
                    volunteers = eventDetails.VolunteerEvents.Select(ve => new
                    {
                        id = ve.Volunteer.ID,
                        fullName = ve.Volunteer.FullName
                    }).ToList()
                };
                return Json(eventData);
            }
            return Json(null); //if empty return null
        }

        // GET: VolunteerEvent/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity");
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email");
            return View();
        }

        // POST: VolunteerEvent/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,VolunteerID,ShiftAttended,ShiftStart,ShiftEnd,NonAttendanceNote")] VolunteerEvent volunteerEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(volunteerEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }

        // GET: VolunteerEvent/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var volunteerEvent = await _context.VolunteerEvents.FindAsync(id);
            if (volunteerEvent == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }

        // POST: VolunteerEvent/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,VolunteerID,ShiftAttended,ShiftStart,ShiftEnd,NonAttendanceNote")] VolunteerEvent volunteerEvent)
        {
            if (id != volunteerEvent.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(volunteerEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VolunteerEventExists(volunteerEvent.EventID))
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
            ViewData["EventID"] = new SelectList(_context.Events, "ID", "EventCity", volunteerEvent.EventID);
            ViewData["VolunteerID"] = new SelectList(_context.Volunteers, "ID", "Email", volunteerEvent.VolunteerID);
            return View(volunteerEvent);
        }




        private void PopulateDDLs(int? selectedYear = null)
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            int year = selectedYear ?? currentYear;

            var events = _context.VolunteerEvents
                .Where(e => e.Event.EventStart.Year == year)
                .GroupBy(e => new { e.Event.EventStart.Month, e.Event.ID }) //Group by the month and the unique event ID
                .Select(g => g.Key.Month) //Select the key of the unique event
                .GroupBy(m => m) //then group them all together by month so if there is 2 events in march, march will display March(2)
                .Select(g => new { Month = g.Key, Count = g.Count() }) //Only select the events in the required months
                .ToList();

            ViewData["selMonth"] = Enumerable.Range(1, 12)
               .Select(m => new SelectListItem
               {
                   Value = m.ToString(),
                   Text = events.Any(e => e.Month == m) //If events.Any(current month) then display how many events
                       ? $"{new DateTime(1, m, 1).ToString("MMMM")} ({events.Where(e => e.Month == m).Sum(e => e.Count)})"
                       : new DateTime(1, m, 1).ToString("MMMM"),
                   Selected = (m == currentMonth)
               }).ToList();

            ViewData["selYear"] = Enumerable.Range(currentYear - 3, 4)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = (y == currentYear)
                }).ToList();
            
        }
        private bool VolunteerEventExists(int id)
        {
            return _context.VolunteerEvents.Any(e => e.EventID == id);
        }
    }
}
