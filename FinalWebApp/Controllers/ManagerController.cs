using Microsoft.AspNetCore.Mvc;

namespace FinalWebApp.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
