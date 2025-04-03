using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TVAttendance.Data;

namespace TVAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TomorrowsVoiceContext _context;


        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !User.IsInRole("Admin"))
            {
                return Forbid(); // Deny access if not an Admin
            }

            var users = _userManager.Users.ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Convert both searchTerm and the user's email/user name to lowercase for case-insensitive comparison
                searchTerm = searchTerm.ToLower();

                users = users.Where(u => u.Email.ToLower().Contains(searchTerm) || u.UserName.ToLower().Contains(searchTerm)).ToList();
            }

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "None/User";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

            return View(users);
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            if (userId == null && newRole == null)
            {
                TempData["Message"] = $"Please Select Proper Information!";
                TempData["MessageType"] = "error"; // Success notification
                return RedirectToAction("Index");
            }

            if (userId == null)
            {
                TempData["Message"] = $"Please Select A Proper User!";
                TempData["MessageType"] = "error"; // Success notification
                return RedirectToAction("Index");
            }

            if (newRole == "None")
            {
                TempData["Message"] = $"Please Select A Proper Role!";
                TempData["MessageType"] = "error"; // Success notification
                return RedirectToAction("Index");
            }


            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !User.IsInRole("Admin"))
            {
                return Forbid(); // Ensure only Admins can change roles
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Check if the user is trying to assign themselves the same role
            if (currentRoles.Contains(newRole))
            {
                TempData["Message"] = $"User is already assigned to the '{newRole}' role!";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Prevent removing the last Admin from the system
            if (currentRoles.Contains("Admin") && newRole != "Admin")
            {
                var totalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (totalAdmins <= 1)
                {
                    TempData["Message"] = "Admin Role Change Canceled: Cannot remove the last Admin.";
                    TempData["MessageType"] = "error"; // Error notification
                    return RedirectToAction("Index");
                }
            }

            // Remove all roles before adding the new one
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // If the new role isn't empty, assign it
            if (!string.IsNullOrEmpty(newRole))
            {
                await _userManager.AddToRoleAsync(user, newRole);
                TempData["Message"] = $"Successfully updated {user.Email} to {newRole}!";
                TempData["MessageType"] = "success"; // Success notification
            }
            else
            {
                TempData["Message"] = $"Successfully removed Role from {user.Email}.";
                TempData["MessageType"] = "info"; // Info notification
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PendingAccounts()
        {
            var allUsers = _userManager.Users.ToList();
            var requiredRoles = new List<string> { "User", "Director", "Supervisor", "Admin" };
            var unassignedUsers = new List<IdentityUser>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any(role => requiredRoles.Contains(role)))
                {
                    unassignedUsers.Add(user);
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
        public async Task<IActionResult> AcceptUser(string id)
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
            if (volunteer == null)
            {
                return NotFound();
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _context.SaveChangesAsync();

            TempData["Message"] = "User accepted and assigned role 'User'.";
            return RedirectToAction("PendingAccounts");
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

            TempData["Message"] = "User denied and removed.";
            return RedirectToAction("PendingAccounts");
        }

        public async Task<IActionResult> PendingAccountDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Fetch the volunteer details (assuming a relation between user and volunteer)
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == user.Email);

            if (volunteer == null)
            {
                return NotFound();
            }

            return View(volunteer);
        }
    }
}
