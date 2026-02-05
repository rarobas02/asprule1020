using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]

    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult New()
        {
            return View();
        }
    }
}
