using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class CommentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
