using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize(Roles = "Administrator")] //control complet doar pentru admin
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PanelController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalProjects = await db.Projects.CountAsync();
            ViewBag.TotalTasks = await db.Tasks.CountAsync();

            var recentProjects = await db.Projects
                                         .Include(p => p.ProjectMembers)
                                         .OrderByDescending(p => p.Id)
                                         .Take(5)
                                         .ToListAsync();
            return View(recentProjects);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var userProjects = db.ProjectMembers.Where(pm => pm.UserId == id);
                db.ProjectMembers.RemoveRange(userProjects);

                var userComments = db.Comments.Where(c => c.UserId == id);
                db.Comments.RemoveRange(userComments);

                await _userManager.DeleteAsync(user); 

                TempData["message"] = "User permanently deleted.";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Projects()
        {
            var projects = await db.Projects
                                   .Include(p => p.ProjectMembers)
                                   .OrderByDescending(p => p.Id)
                                   .ToListAsync();
            return View(projects);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await db.Projects.FindAsync(id);
            if (project != null)
            {
                db.Projects.Remove(project); // cascade delete va sterge taskurile si comentariile
                await db.SaveChangesAsync();

                TempData["message"] = "The project has been forced deleted by Admin.";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("Projects");
        }
    }
}