using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementApp.Models;
using TaskManagementApp.Services;

namespace TaskManagementApp.Controllers
{
    public class ProjectsController: Controller 
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IProjectSummaryService _summaryService;

        public ProjectsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IProjectSummaryService summaryService
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _summaryService = summaryService;
        }


        //1. afisrea proiectelor (index)
        //afiseaza proiectele la care utilizatorul curent este membru, ordonate dupa data.
        //administratorul vede tot proiectele din care face parte pentru ca pe toate le poate vedea in panel
        [Authorize(Roles = "Membru,Administrator")] //doar utilizatorii autentificati pot accesa
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            List<Project> projects;
           
           
            projects = db.Projects
                 .Include(p => p.Organizer)
                 .Include(p => p.ProjectMembers)
                 .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
                 .OrderByDescending(p => p.Date)
                 .ToList();
          

            ViewBag.Projects = projects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(projects);
        }

        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Show(int id)
        {

            var project = db.Projects
                .Include(p => p.Organizer)
                .Include(p => p.Tasks)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .Include(p => p.ProjectSummaries)
                .FirstOrDefault(p => p.Id == id);

            if (project is null)
            {
                return NotFound();
            }

            SetAccessRights(project);

            var userId = _userManager.GetUserId(User);

            bool esteMembru = project.ProjectMembers.Any(pm => pm.UserId == userId);

            if (!esteMembru && project.OrganizerId != userId && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have access to this project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult New()
        {
            Project project = new Project();

            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult New(Project project)
        {
            project.Date = DateTime.Now;

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["message"] = "You have to be logged in to create a project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            project.OrganizerId = userId;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                TempData["message"] = string.Join(", ", errors);
                TempData["messageType"] = "alert-danger";
                return View(project);
            }


            if (ModelState.IsValid)
            {
                project.ProjectMembers = new List<ProjectMember>();

                project.ProjectMembers.Add(new ProjectMember
                {
                    UserId = userId,
                    IsAccepted = true
                });

                db.Projects.Add(project);
                db.SaveChanges();

                TempData["message"] = "Project was created";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index");
            }
            else {

                return View(project);

            }

        }

        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Edit(int id)
        {
            var project = db.Projects.Find(id);

            if (project is null)
                return NotFound(); 

            if (project.OrganizerId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
            {
                return View(project);
            }

            TempData["message"] = "You don't have permission to edit this project";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }




        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Edit(int id, Project requestProject)
        {
            var project = db.Projects.Find(id);

            if (project is null)
                return NotFound();

            if (project.OrganizerId != _userManager.GetUserId(User) && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have permission to edit this project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                project.Title = requestProject.Title;
                project.Description = requestProject.Description;

                db.SaveChanges();

                TempData["message"] = "Project successfully updated!";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Show", new { id = project.Id });
            }

            return View(requestProject);
        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult Delete(int id)
        {
            var project = db.Projects
                            .Include(p => p.ProjectMembers)
                            .FirstOrDefault(p => p.Id == id);


            if (project is null)
            {
                return NotFound();
            }

            if( project.OrganizerId != _userManager.GetUserId(User) && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have permission to delete the project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            db.Projects.Remove(project);
            db.SaveChanges();

            TempData["message"] = "Project successfully deleted!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }




        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult AddMember(int projectId)
        {
            var project = db.Projects
                    .Include(p => p.ProjectMembers)
                    .FirstOrDefault(p => p.Id == projectId);


            if (project is null)
                return NotFound();

            if (project.OrganizerId != _userManager.GetUserId(User) && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have permission to add people to the project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            //lista utilizatorilor disponibili pentru a fi adaugati ca membri
            var memberIds = project.ProjectMembers.Select(pm => pm.UserId).ToList();

            // lista userilor care NU sunt membri ai proiectului
            var availableUsers = db.Users
                                   .Where(u => !memberIds.Contains(u.Id))
                                   .OrderBy(u => u.UserName)
                                   .ToList();

            ViewBag.Users = availableUsers;

            return View(project);
        }



        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public async Task<IActionResult> AddMember(int projectId, string userId)
        {
            
            var project = await db.Projects
                                  .Include(p => p.ProjectMembers)
                                  .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (project.OrganizerId != currentUserId && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You can't add members to the project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }

            var existingMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId);

            if (existingMember == null)
            {

                var newMember = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userId,
                    IsAccepted = false
                };
                db.ProjectMembers.Add(newMember);

                var notif = new Notification
                {
                    UserId = userId,               
                    SenderId = currentUserId,      
                    Text = $"Invited you to join their project: '{project.Title}'.",
                    Type = "Invite",               
                    RelatedEntityId = projectId,   
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                db.Notifications.Add(notif);

                await db.SaveChangesAsync();

                TempData["message"] = "Invite sent!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
       

                if (existingMember.IsAccepted)
                {
                    TempData["message"] = "User is already a member of the project.";
                    TempData["messageType"] = "alert-warning";
                }
                else
                {
                    TempData["message"] = "User already has a pending request.";
                    TempData["messageType"] = "alert-info";
                }
            }

            return RedirectToAction("Show", new { id = projectId });
        }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public IActionResult RemoveMember(int projectId, string memberId)
        {
            var project = db.Projects.Include(p => p.ProjectMembers)
                                       .FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            //verificare drepturi: doar organizatorul sau administratorul pot elimina membri
            if (project.OrganizerId != _userManager.GetUserId(User) && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have permission to remove members from this project!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }

            //gasim membrul de eliminat
            var memberToRemove = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == memberId);

            if (memberToRemove != null)
            {
                db.ProjectMembers.Remove(memberToRemove);
                db.SaveChanges();

                TempData["message"] = "The member has been eliminated from the project!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "The member has not been found as part of the project!";
                TempData["messageType"] = "alert-warning";
            }
            return RedirectToAction("Show", new { id = projectId });
        }


        private void SetAccessRights(Project project)
        {
             ViewBag.UserCurent = _userManager.GetUserId(User);
             ViewBag.EsteAdmin = User.IsInRole("Administrator");
             ViewBag.EsteOrganizator = project.OrganizerId == ViewBag.UserCurent;
         }

        [HttpPost]
        [Authorize(Roles = "Membru,Administrator")]
        public async Task<IActionResult> GenerateSummary(int projectId)
        {
            var project = db.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.User)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            if (project.OrganizerId != _userManager.GetUserId(User) &&
                !User.IsInRole("Administrator"))
            {
                TempData["message"] = "You don't have permission to generate an AI Summary report.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }

            var content = await _summaryService.GenerateSummaryAsync(project);

            var summary = new Summary
            {
                ProjectId = projectId,
                Content = content,
                GeneratedAt = DateTime.Now
            };

            db.ProjectSummaries.Add(summary);
            db.SaveChanges();

            TempData["message"] = "AI Summary report has been updated.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", new { id = projectId });
        }

    }
}