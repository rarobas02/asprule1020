using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    public class VerifyController : Controller
    {
        [Area("Client")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
