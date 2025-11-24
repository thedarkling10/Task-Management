using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class TeamsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
