using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;
using TVAttendance.Models;

namespace TVAttendance.Controllers
{
    public class PendingVolunteersController : Controller
    {
        private readonly TomorrowsVoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PendingVolunteersController(UserManager<IdentityUser> userManager, TomorrowsVoiceContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: PendingVolunteers/CreatePending
        public async Task<IActionResult> CreatePending()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new PendingVolunteer
            {
                Email = user.Email,
                RegisterDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePending(PendingVolunteer model)
        {
            var user = await _userManager.GetUserAsync(User);

            // Assign email and user ID before validation
            model.Email = user.Email;
            model.RegisterDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.PendingVolunteers.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("PendingApproval");
            }

            return View(model);
        }

        public IActionResult PendingApproval()
        {
            return View(); // thank-you / waiting message
        }

        private bool PendingVolunteerExists(int id)
        {
            return _context.PendingVolunteers.Any(e => e.ID == id);
        }
    }
}
