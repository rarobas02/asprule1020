using asprule1020.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    public class VerifyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(string transId)
        {
            var normalizedTransId = transId?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedTransId))
            {
                ViewData["LookupMessage"] = "Transaction id does not found.";
                return View(model: null);
            }

            var register = _unitOfWork.Register.Get(x => x.TransId == normalizedTransId);
            if (register == null)
            {
                ViewData["LookupMessage"] = "Transaction id does not found.";
                ViewData["SearchedTransId"] = normalizedTransId;
                return View(model: null);
            }

            return View(register);
        }
    }
}
