using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        // 1. afisare Inbox
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var notifications = await db.Notifications
                                        .Include(n => n.Sender) //Ca sa vedem cine a trimis
                                        .Where(n => n.UserId == userId)
                                        .OrderByDescending(n => n.CreatedDate)
                                        .ToListAsync();

            return View(notifications);
        }

        // 2. acceptare Invitatie
        [HttpPost]
        public async Task<IActionResult> AcceptInvite(int notificationId, int projectId)
        {
            var userId = _userManager.GetUserId(User);

            // cautam membrul care e pending (IsAccepted = false)
            var member = await db.ProjectMembers
                                 .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member != null)
            {
                // IL ACCEPTAM IN PROIECT
                member.IsAccepted = true;

                // Stergem notificarea pentru ca a fost rezolvata
                var notif = await db.Notifications.FindAsync(notificationId);
                if (notif != null) db.Notifications.Remove(notif);

                await db.SaveChangesAsync();
                TempData["message"] = "Ai intrat în proiect cu succes!";
                return RedirectToAction("Show", "Projects", new { id = projectId });
            }

            TempData["message"] = "Eroare: Nu s-a găsit invitația.";
            return RedirectToAction("Index");
        }

        // 3. mark as read (pentru comentarii)
        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notif = await db.Notifications.FindAsync(id);
            if (notif != null && notif.UserId == _userManager.GetUserId(User))
            {
                notif.IsRead = true;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
