using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
