using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System.Diagnostics;
using System.Security.Claims;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Data.Migrations;
using TVAttendance.Models;
using TVAttendance.ViewModels;

namespace TVAttendance.Controllers
{
    public class HomeController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<HomeController> logger, TomorrowsVoiceContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (!roles.Any()) // No roles assigned to the user
                    {
                        // Check if they have already filled out their pending volunteer form
                        bool pendingExists = _context.PendingVolunteers.Any(v => v.Email == user.Email);

                        if (!pendingExists)
                        {
                            // If no pending volunteer form, redirect to CreatePending
                            return RedirectToAction("CreatePending", "PendingVolunteers");
                        }
                        else
                        {
                            // If a pending volunteer form exists, redirect to PendingApproval
                            return RedirectToAction("PendingApproval", "PendingVolunteers");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error for debugging purposes (optional)
                    _logger.LogError($"Error while checking roles for user {User.Identity.Name}: {ex.Message}");

                    // Set a TempData message to inform the user of the error
                    TempData["Message"] = "Account Compromised. Please Contact System Administrator. (Resetting Cookies May Fix This!)";
                    TempData["MessageType"] = "error"; // Error notification type

                    // Redirect to an error page or the home page
                    return RedirectToAction("Error", "Home"); // Replace with the appropriate action if needed
                }
            }
            var userEmail = User.Identity.Name; // Get the logged-in user's email
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var events = _context.Events
                 .Include(e => e.Shifts)
                     .ThenInclude(e => e.ShiftVolunteers)
                     .Where(s => s.EventOpen)
                 .ToList();
            //order by datetime and take the first 5 events
            List<Event> mostRecentEvents = events.OrderByDescending(d =>
            d.EventDate)
                .Take(5)
                .ToList();

            var singers = _context.Singers
               .Include(s => s.Chapter)
               .ToList();

            var volunteers = _context.Volunteers
                .Include(v => v.ShiftVolunteers)
                    .ThenInclude(s => s.Shift)
                .ToList();

            var sessions = _context.Sessions
               .Include(s => s.Chapter)
                   .ThenInclude(c => c.Directors)
               .Include(s => s.Chapter)
                   .ThenInclude(c => c.Singers)
               .Include(s => s.SingerSessions)
                   .ThenInclude(ss => ss.Singer)
               .ToList();

            var shifts = _context.ShiftVolunteers
                .Include(s=>s.Shift)
                .ThenInclude(s=>s.Event)
            .ToList();

            var userRolesDict = new Dictionary<string, string>();

            if (userRoles.Contains("Admin"))
            {
                var users = _userManager.Users.Take(5).ToList(); //get 5 users
                List<UsersVM> usersVM = new List<UsersVM>(); //create a new empty list so we can access it outside of foreach's scope
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user); //get all roles
                    userRolesDict[user.Id] = roles.FirstOrDefault() ?? "None/User"; //from AdminController
                    usersVM.Add(new UsersVM //add a new user for each of the users in the list with their email n role
                    {
                        Email = user.Email,
                        Role = userRolesDict[user.Id]

                    });
                }
                ViewBag.Users = usersVM; //ViewBag will be property you access in Home/Index.cshtml

                return View(mostRecentEvents); //returned VM's are what you use for partial views
            }

            if (userRoles.Contains("Director"))
            {

                //shows sessions by 5 most recent 
                var dirSessions = sessions.OrderByDescending(s => s.Date).Take(5).ToList();
                List<SessionVM> attendanceVM = new List<SessionVM>();
                foreach (var session in dirSessions)
                {
                    attendanceVM.Add(new SessionVM
                    {
                        ID = session.ID,
                        Chapter = session.Chapter,
                        Date = session.Date,
                        AttendanceRate = session.AttendanceRate,
                    });
                }
                ViewBag.Sessions = attendanceVM;
                return View(mostRecentEvents);
            }

            if (userRoles.Contains("Volunteer"))
            {
                //Redirect()
            }

            if (userRoles.Contains("User"))
            {
                //Shows the shifts closest to today (5) VM
                var userShifts = shifts.OrderByDescending(u => u.Shift.ShiftStartDate).Take(5).ToList();
                List<ShiftsVM> shiftsVM = new List<ShiftsVM>();
                foreach (var userShift in userShifts)
                {
                    shiftsVM.Add(new ShiftsVM
                    {
                        Name = userShift.Shift.Event.EventName,
                        Time = userShift.Shift.Event.EventTime.ToString(),
                        Date = userShift.Shift.Event.EventDate.ToString()
                    });
                }
                //Shows events (5) VM
                var userEvents = events.OrderByDescending(s => s.EventStartDate).Take(5).ToList();
                List<EventsVM> eventsVMs = new List<EventsVM>();
                foreach (var e in userEvents)
                {
                    string abbProvince = e.ProvinceAbbreviation(e.EventProvince.ToString());
                    eventsVMs.Add(new EventsVM
                    {
                        ID = e.ID,
                        Name = e.EventName,
                        City = e.EventCity,
                        Date = e.EventDate,
                        Province = abbProvince,
                    });
                    ViewBag.Events = eventsVMs;
                }
                //Shows Volunteer details W/O shifts 
                //Get this user based off their email
                var thisUser = volunteers.Where(v => v.Email == userEmail).FirstOrDefault();
                ViewBag.Volunteer = thisUser;
                //return View(thisUser);
                return View(shiftsVM);
            }
            //ChoirDBVM choirdash = new ChoirDBVM()
            //{
            //    Singers = singers,
            //    Sessions = sessions
            //};

            //EventDBVM eventdash = new EventDBVM()
            //{
            //    Events = mostRecentEvents,
            //    Shifts = shifts,
            //    Volunteers = volunteers
            //};

            //DashboardVM dashboard = new DashboardVM()
            //{
            //    ChoirDash = choirdash,
            //    EventDash = eventdash
            //};

            return View();
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}