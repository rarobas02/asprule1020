using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewAll()
        {
            return View();
        }
    }
}
