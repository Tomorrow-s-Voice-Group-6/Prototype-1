using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
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


            var events = _context.Events
                 .Include(e => e.Shifts)
                     .ThenInclude(e => e.ShiftVolunteers)
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

            ChoirDBVM choirdash = new ChoirDBVM()
            {
                Singers = singers,
                Sessions = sessions
            };

            EventDBVM eventdash = new EventDBVM()
            {
                Events = events,
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
