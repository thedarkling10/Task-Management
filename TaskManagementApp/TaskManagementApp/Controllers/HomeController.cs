using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Adaugam referintele catre db si UserManager
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        // Le injectam in constructor
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            db = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard(string statusFilter)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Proiectele în care apare utilizatorul
            var userProjects = db.Projects
                .Include(p => p.ProjectMembers)
                .Where(p => p.OrganizerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId && pm.IsAccepted))
                .OrderByDescending(p => p.Date)
                .ToList();

            // 2. Task-urile asignate utilizatorului
            var tasksQuery = db.Tasks
                .Include(t => t.Project)
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            var allUserTasks = tasksQuery.ToList();

            // 3. Deadline-uri apropiate (7 zile)
            var upcoming = allUserTasks
                .Where(t => t.EndDate >= DateTime.Now && t.EndDate <= DateTime.Now.AddDays(7))
                .OrderBy(t => t.EndDate)
                .ToList();

            // Pas?m datele prin ViewBag (F?r? model nou)
            ViewBag.Projects = userProjects;
            ViewBag.Upcoming = upcoming;
            ViewBag.SelectedFilter = statusFilter;

            return View(allUserTasks);
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