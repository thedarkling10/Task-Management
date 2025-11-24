using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class SummariesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
