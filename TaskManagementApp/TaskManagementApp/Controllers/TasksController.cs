using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    public class TasksController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Show(int Id)
        {
            var task = db.Tasks
                .Include(t => t.User)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Organizer)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefault(t => t.Id == Id);

            if (task == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool esteMembru = db.ProjectMembers
                                .Any(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (task.Project.OrganizerId != userId && !esteMembru && !User.IsInRole("Administrator"))
            {
                TempData["ErrorMessage"] = "Nu ai permisiunea să vizualizezi acest task.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Projects", new { id = task.ProjectId});
            }

            SetAccessRights(task);

            return View(task);
        }


        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult New(int projectId)
        {

            var project = db.Projects 
                             .Include(p => p.ProjectMembers)
                             .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var members = db.ProjectMembers
                            .Where(pm => pm.ProjectId == projectId)
                            .Include(pm => pm.User)
                            .Select(pm => pm.User)
                            .ToList();

            ViewBag.Users = members;


            var userId = _userManager.GetUserId(User);

            bool esteMembru = project.ProjectMembers
                                .Any(pm => pm.UserId == userId);

            if(!esteMembru && project.OrganizerId != userId)
            {
                TempData["ErrorMessage"] = "Nu ai permisiunea să adaugi task-uri în acest proiect.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Projects", new { id = projectId });
            }

            var task = new Models.Task
            {
                ProjectId = projectId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };
            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult New(Models.Task task)
        {

            task.Comments = new List<Comment>();

            var project = db.Projects
                             .Include(p => p.ProjectMembers)
                             .FirstOrDefault(p => p.Id == task.ProjectId);

            if(project == null)
                return NotFound();

            task.ProjectId = project.Id;

            task.Comments = new List<Comment>();

            if (task.StartDate > task.EndDate)
            {
                ModelState.AddModelError("StartDate", "Data de început nu poate fi după data de sfârșit.");
            }


            if (ModelState.IsValid)
            {

                db.Tasks.Add(task);
                db.SaveChanges();
                TempData["message"] = "Task-ul a fost creat!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", "Projects", new { id = task.ProjectId });
            }
            else
            {
                TempData["message"] = "Task-ul NU a fost creat!";
                TempData["messageType"] = "alert-danger";
            }
            return View(task);
        }


        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Edit(int Id)
        {
            var task = db.Tasks 
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == Id);

            var users = db.ProjectMembers
                        .Where(pm => pm.ProjectId == task.ProjectId)
                        .Include(pm => pm.User)
                        .Select(pm => pm.User)
                        .ToList();

            ViewBag.Users = users;

            if (task == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool esteMembru = db.ProjectMembers
                                .Any(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (!esteMembru && task.Project.OrganizerId != userId && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "Nu aveți dreptul să editați acest task!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Projects", new { id = task.ProjectId });
            }

            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Edit(int id, Models.Task requestTask)
        {
            var task = db.Tasks
                           .Include(t => t.Project)
                            .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool esteAdmin = User.IsInRole("Administrator");
            bool esteOrganizator = task.Project.OrganizerId == userId;
            bool esteAsignat = task.UserId == userId;

            if (!esteAdmin && !esteOrganizator && !esteAsignat)
                return Forbid();

            if (requestTask.StartDate > requestTask.EndDate)
            {
                ModelState.AddModelError("StartDate", "Start date can't be the same as End date.");
            }

            if (!ModelState.IsValid)
            {
                requestTask.ProjectId = task.ProjectId;  // pentru a pastra ProjectId in view   
                requestTask.Project= task.Project;      // pentru a pastra Project in view
                return View(requestTask);
            }

            if (esteAdmin || esteOrganizator)
            {
                // POT MODIFICA TOT
                task.Title = requestTask.Title;
                task.Description = requestTask.Description;
                task.Content = requestTask.Content;
                task.StartDate = requestTask.StartDate;
                task.EndDate = requestTask.EndDate;
                task.UserId = requestTask.UserId;
                task.Status = requestTask.Status;
            }
            else if (esteAsignat)
            {
                // DOAR STATUS
                task.Status = requestTask.Status;
            }

            db.SaveChanges();

            TempData["message"] = "Task-ul a fost editat!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", "Projects", new { id = task.ProjectId });

        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Delete(int id)
        {
            var task = db.Tasks.Find(id);

            if (task == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool esteMembru = db.ProjectMembers
                .Any(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (!esteMembru && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți acest task!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Projects", new { id = task.ProjectId });
            }

            db.Tasks.Remove(task);
            db.SaveChanges();

            TempData["message"] = "Task-ul a fost șters!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", "Projects", new { id = task.ProjectId });
        }

        private void SetAccessRights(Models.Task task)
        {
            var userId = _userManager.GetUserId(User);

            ViewBag.UserCurent = userId;
            ViewBag.EsteAdmin = User.IsInRole("Administrator");
            ViewBag.EsteOrganizator = task.Project.OrganizerId == userId;
            ViewBag.EsteMembru = db.ProjectMembers.Any(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);
        }
    }
}
