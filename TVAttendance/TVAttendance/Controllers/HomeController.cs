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
        public HomeController(ILogger<HomeController> logger, TomorrowsVoiceContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    //Redirect to the Login action in the Account controller within the Identity area
            //    string loginUrl = Url.Action("Login", "Account", new { area = "Identity" });
            //    return Redirect(loginUrl);
            //}
            //Index will return a page based on the following values:
            var userEmail = User.Identity.Name; // Get the logged-in user's email
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var events = _context.Events
                 .Include(e => e.Shifts)
                     .ThenInclude(e => e.ShiftVolunteers)
                 .ToList();
            //order by datetime and take the first 5 events
            List<Event> mostRecentEvents = events.OrderBy(d =>
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
            .ToList();
            if(userRoles.Contains("Admin"))
            { 
                var users = _userManager.Users.ToList();
                List<UsersVM> usersVM = new List<UsersVM>();   
                foreach (var user in users)
                {
                    usersVM.Add(new UsersVM
                    {
                        Name = user.UserName,
                        Email = user.Email,
                        Role = user.GetType().Name
                    });
                }
                ViewBag.Users = usersVM;
                return View(mostRecentEvents);
            }
            if (userRoles.Contains("Director"))
            {

            }
            if (userRoles.Contains("Volunteer"))
            {
                //Redirect()
            }
            if (userRoles.Contains("User"))
            {

            }
            ChoirDBVM choirdash = new ChoirDBVM()
            {
                Singers = singers,
                Sessions = sessions
            };

            EventDBVM eventdash = new EventDBVM()
            {
                Events = mostRecentEvents,
                Shifts = shifts,
                Volunteers = volunteers
            };

            DashboardVM dashboard = new DashboardVM()
            {
                ChoirDash = choirdash,
                EventDash = eventdash
            };

            return View(dashboard);
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
