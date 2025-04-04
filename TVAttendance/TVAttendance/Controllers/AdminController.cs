using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TVAttendance.Data;
using TVAttendance.Models;
using static TVAttendance.Utilities.EmailService;

namespace TVAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TomorrowsVoiceContext _context;

        private readonly IEmailSender _emailSender;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, TomorrowsVoiceContext context, IEmailSender emailSender, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !User.IsInRole("Admin"))
            {
                return Forbid(); // Deny access if not an Admin
            }

            // Retrieve all users
            var users = _userManager.Users.AsQueryable();

            // Filter by searchTerm if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u => u.Email.ToLower().Contains(searchTerm) || u.UserName.ToLower().Contains(searchTerm));
            }

            // Filter by roles (only allowed roles: User, Director, Supervisor, Admin)
            var allowedRoles = new List<string> { "User", "Director", "Supervisor", "Admin" };
            var usersWithRoles = new List<IdentityUser>();

            // Fetch roles and filter accordingly
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(r => allowedRoles.Contains(r)))
                {
                    usersWithRoles.Add(user);
                }
            }

            // Pagination - Show only 10 users per page
            var usersToShow = usersWithRoles.Skip((page - 1) * 10).Take(10).ToList();

            // Get user roles for the view
            var userRoles = new Dictionary<string, string>();
            foreach (var user in usersToShow)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "None/User";
            }

            // Set up ViewBag for user roles and available roles for role dropdown
            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = allowedRoles;

            // Set the total number of pages for pagination
            var totalUsers = usersWithRoles.Count;
            var totalPages = (int)Math.Ceiling(totalUsers / 10.0);
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View(usersToShow);
        }




        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole, string searchTerm = "")
        {
            if (userId == null && newRole == null)
            {
                TempData["Message"] = "Please Select Proper Information!";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (userId == null)
            {
                TempData["Message"] = "Please Select A Proper User!";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (newRole == "None")
            {
                TempData["Message"] = "Please Select A Proper Role!";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(newRole))
            {
                TempData["Message"] = $"User is already assigned to the '{newRole}' role!";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (currentRoles.Contains("Admin") && newRole != "Admin")
            {
                var totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (totalAdmins <= 1)
                {
                    TempData["Message"] = "Admin Role Change Canceled: Cannot remove the last Admin.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(newRole))
            {
                await _userManager.AddToRoleAsync(user, newRole);
                TempData["Message"] = $"Successfully updated {user.Email} to {newRole}!";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = $"Successfully removed Role from {user.Email}.";
                TempData["MessageType"] = "info";
            }

            // Filter the roles to show only specific ones
            var allowedRoles = new List<string> { "User", "Director", "Supervisor", "Admin" };
            var filteredRoles = allowedRoles.Where(r => r.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();

            ViewBag.Roles = filteredRoles;

            return RedirectToAction("Index");
        }


        // Method to check if a PendingVolunteer exists and if the information is filled
        public bool IsAccountInfoFilled(string email)
        {
            var volunteer = _context.PendingVolunteers.FirstOrDefault(v => v.Email == email);
            return volunteer != null &&
                   !string.IsNullOrEmpty(volunteer.FirstName) &&
                   !string.IsNullOrEmpty(volunteer.LastName) &&
                   !string.IsNullOrEmpty(volunteer.Phone);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingAccounts()
        {
            // Asynchronously get all users
            var allUsers = await _userManager.Users.ToListAsync();
            var requiredRoles = new List<string> { "User", "Director", "Supervisor", "Admin" };
            var unassignedUsers = new List<dynamic>();

            // Loop through users and check their roles and account information
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any(role => requiredRoles.Contains(role)))
                {
                    // Check if account information is filled for this user
                    var accountInfoFilled = IsAccountInfoFilled(user.Email);

                    // Add user with account info status
                    unassignedUsers.Add(new
                    {
                        User = user,
                        AccountInfoFilled = accountInfoFilled
                    });
                }
            }

            return View(unassignedUsers);
        }

        //public async Task<IActionResult> UserDetails(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == id);
        //    if (volunteer == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(volunteer);
        //}

        [HttpPost]
        public async Task<IActionResult> EmailAcceptUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    TempData["Message"] = "Failed to confirm email.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index"); // or UnassignedUsers
                }
            }

            TempData["Message"] = "Email marked as confirmed.";
            TempData["MessageType"] = "success";
            return RedirectToAction("PendingAccounts");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmVolunteer(int id)
        {
            var pendingVolunteer = await _context.PendingVolunteers
                .FirstOrDefaultAsync(pv => pv.ID == id);

            if (pendingVolunteer == null)
            {
                return NotFound();
            }

            // Create a new Volunteer from PendingVolunteer data
            var volunteer = new Volunteer
            {
                FirstName = pendingVolunteer.FirstName,
                LastName = pendingVolunteer.LastName,
                Phone = pendingVolunteer.Phone,
                Email = pendingVolunteer.Email,
                DOB = pendingVolunteer.DOB,
                RegisterDate = pendingVolunteer.RegisterDate
            };

            // Add the volunteer to the database
            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            // Fetch the User and update roles (assign "User" role)
            var user = await _userManager.FindByEmailAsync(pendingVolunteer.Email);
            if (user != null)
            {
                var role = await _roleManager.FindByNameAsync("User");
                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            // Remove the PendingVolunteer entry from the pending list
            _context.PendingVolunteers.Remove(pendingVolunteer);
            await _context.SaveChangesAsync();

            // Send confirmation email
            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Account Confirmed", "Your account has been successfully confirmed and activated as a volunteer.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {user.Email}: {ex.Message}");
            }

            TempData["Message"] = "Volunteer account confirmed and email sent.";
            TempData["MessageType"] = "success";

            return RedirectToAction("PendingAccounts", "Admin"); // Redirect to the admin dashboard
        }

        [HttpPost]
        public async Task<IActionResult> DenyUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == user.Email);
            if (volunteer != null)
            {
                _context.Volunteers.Remove(volunteer);
            }

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();

            // Send denial email
            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Account Denied", "Your application to become a volunteer has been denied.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {user.Email}: {ex.Message}");
            }

            TempData["Message"] = "User denied and removed, email sent.";
            TempData["MessageType"] = "error"; // Denial notification

            return RedirectToAction("PendingAccounts");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingAccountDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Use Email to find the pending volunteer submission
            var pendingVolunteer = await _context.PendingVolunteers
                .FirstOrDefaultAsync(pv => pv.Email == user.Email);

            if (pendingVolunteer == null)
            {
                return NotFound(); // No pending submission found
            }

            return View(pendingVolunteer);
        }

    }
}
