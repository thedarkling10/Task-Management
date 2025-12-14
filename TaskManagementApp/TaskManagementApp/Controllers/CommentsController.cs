using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize(Roles = "Membru,Administrator")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. afiseaza comentariile pentru un task specific
        public IActionResult Index(int taskId)
        {
            var task = db.Tasks
                .Include(t => t.Project)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                TempData["message"] = "Task-ul nu a fost găsit!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!UserHasAccessToProject(task.ProjectId))
            {
                TempData["message"] = "Nu aveți acces la acest task!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }

            ViewBag.Task = task;
            SetAccessRights(task.Project);

            return View(task.Comments
                .OrderByDescending(c => c.Date)
                .ToList());
        }


        // 2. NEW – GET – formular creare comentariu
        public IActionResult New(int taskId)
        {
            var task = db.Tasks
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == taskId);

            if (task is null)
            {
                TempData["message"] = "Task-ul nu a fost găsit!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!UserHasAccessToProject(task.ProjectId))
            {
                TempData["message"] = "Nu aveți acces la acest task!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }

            ViewBag.Task = task;

            Comment comment = new Comment
            {
                TaskId = taskId
            };

            return View(comment);
        }


        // 3. NEW – POST – salvează comentariul nou creat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(Comment comment)
        {
            var task = db.Tasks
                .Include(t => t.Project)
                .FirstOrDefault(t => t.Id == comment.TaskId);

            if (task is null)
            {
                TempData["message"] = "Task-ul nu există!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!UserHasAccessToProject(task.ProjectId))
            {
                TempData["message"] = "Nu aveți acces la acest task!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Task = task;
                return View(comment);
            }

            comment.UserId = _userManager.GetUserId(User)!;
            comment.Date = DateTime.Now;

            db.Comments.Add(comment);
            db.SaveChanges();

            TempData["message"] = "Comentariul a fost adăugat!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
        }


        // 4. EDIT – GET – formular editare comentariu
        public IActionResult Edit(int id)
        {
            var comment = db.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .Include(c => c.User)
                .FirstOrDefault(c => c.Id == id);

            if (comment is null)
            {
                TempData["message"] = "Comentariul nu există!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!CanModifyComment(comment))
            {
                TempData["message"] = "Nu aveți dreptul să editați acest comentariu!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
            }

            ViewBag.Task = comment.Task;

            return View(comment);
        }


        // 5. EDIT – POST – salvează modificările comentariului
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Comment requestComment)
        {
            var comment = db.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefault(c => c.Id == id);

            if (comment is null)
            {
                TempData["message"] = "Comentariul nu există!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!CanModifyComment(comment))
            {
                TempData["message"] = "Nu aveți dreptul să editați acest comentariu!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Task = comment.Task;
                return View(requestComment);
            }

            comment.Content = requestComment.Content;
            comment.Date = DateTime.Now;

            db.SaveChanges();

            TempData["message"] = "Comentariul a fost actualizat!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
        }


        // 6. DELETE – POST – șterge comentariul
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var comment = db.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefault(c => c.Id == id);

            if (comment is null)
            {
                TempData["message"] = "Comentariul nu există!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!CanModifyComment(comment) &&
                comment.Task.Project.OrganizerId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Nu aveți dreptul să ștergeți acest comentariu!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
            }

            var taskId = comment.TaskId;

            db.Comments.Remove(comment);
            db.SaveChanges();

            TempData["message"] = "Comentariul a fost șters!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", "Tasks", new { id = taskId });
        }


        // 7. SHOW – afișează un comentariu specific
        public IActionResult Show(int id)
        {
            var comment = db.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .Include(c => c.User)
                .FirstOrDefault(c => c.Id == id);

            if (comment is null)
            {
                TempData["message"] = "Comentariul nu există!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }

            if (!UserHasAccessToProject(comment.Task.ProjectId))
            {
                TempData["message"] = "Nu aveți acces la acest comentariu!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }

            SetAccessRights(comment.Task.Project);

            return View(comment);
        }


        // helpere pentru verificarea drepturilor de acces

        private bool UserHasAccessToProject(int projectId)
        {
            var userId = _userManager.GetUserId(User);

            var project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return false;

            bool isOrganizer = project.OrganizerId == userId;
            bool isMember = db.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.UserId == userId);
            bool isAdmin = User.IsInRole("Administrator");

            return isOrganizer || isMember || isAdmin;
        }

        private bool CanModifyComment(Comment comment)
        {
            var userId = _userManager.GetUserId(User);
            return comment.UserId == userId || User.IsInRole("Administrator");
        }

        private void SetAccessRights(Project project)
        {
            ViewBag.UserCurent = _userManager.GetUserId(User);
            ViewBag.EsteAdmin = User.IsInRole("Administrator");
            ViewBag.EsteOrganizator = project.OrganizerId == ViewBag.UserCurent;
        }
    }
}
