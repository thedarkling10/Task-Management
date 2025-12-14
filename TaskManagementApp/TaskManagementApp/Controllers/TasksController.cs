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

            var userId = _userManager.GetUserId(User);

            bool esteMembru = project.ProjectMembers
                                .Any(pm => pm.UserId == userId);

            if(!esteMembru && project.OrganizerId != userId)
            {
                TempData["ErrorMessage"] = "Nu ai permisiunea să adaugi task-uri în acest proiect.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Projects", new { id = projectId });
            }

            var task = new Models.Task { ProjectId = projectId };
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

            TempData["DebugTask"] = $"Title: {task.Title}, ProjectId: {task.ProjectId}, StartDate: {task.StartDate}, EndDate: {task.EndDate}";

            var modelErrors = ModelState
                      .Where(ms => ms.Value.Errors.Count > 0)
                      .Select(ms => $"{ms.Key}: {string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage))}")
                      .ToList();

            if (modelErrors.Any())
            {
                TempData["DebugErrors"] = string.Join(" | ", modelErrors);
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
            var task = db.Tasks.Find(id);

            if (task == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if(!User.IsInRole("Administrator"))
            {
                bool esteMembru = db.ProjectMembers
                                    .Any(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);
                if (!esteMembru && task.Project.OrganizerId != userId)
                {
                    TempData["message"] = "Nu aveți dreptul să editați acest task!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Projects", new { id = task.ProjectId });
                }
            }

            if (requestTask.StartDate > requestTask.EndDate)
            {
                ModelState.AddModelError("StartDate", "Data de început nu poate fi după data de sfârșit.");
            }

            if(!ModelState.IsValid)
            {
                return View(requestTask);
            }

            task.Title = requestTask.Title;
            task.Description = requestTask.Description;
            task.Status = requestTask.Status;
            task.Content = requestTask.Content;
            task.StartDate = requestTask.StartDate;
            task.EndDate = requestTask.EndDate;
            task.UserId = requestTask.UserId;

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
