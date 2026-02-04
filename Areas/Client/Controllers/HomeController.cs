using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
    }
}
