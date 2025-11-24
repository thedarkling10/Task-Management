using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
