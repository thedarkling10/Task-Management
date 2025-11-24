using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
