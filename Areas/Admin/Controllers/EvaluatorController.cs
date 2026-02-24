using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EvaluatorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EvaluatorController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Review()
        {
            //var province = User.FindFirstValue("EstProvince");
            //List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == "For Review").ToList();
            return View();
        }
        public IActionResult ReviewItem(Guid? id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound();
            }
            RegisterVM registerVM = new RegisterVM()
            {
                Register = new Register(),
            };
            registerVM.Register = _unitOfWork.Register.Get(u => u.Id == id);
            return View(registerVM);
        }
        public IActionResult Approved()
        {
            return View();
        }
        public IActionResult Reapplication()
        {
            return View();
        }
        public IActionResult Updating()
        {
            return View();
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAllForReview(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == "For Review").ToList();
            return Json(new { data = objRegisterList });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EvaluationResult(Register register)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userFullName = User.FindFirstValue("FullName");
            //_unitOfWork.Register.UpdateEvaluator(register, userFullName);
            //_unitOfWork.Register.Save();
            //return Json(new { success = true, message = "Evaluation updated successfully" });
            var evaluatorFullName = string.Join(" ", new[]
            {
                User.FindFirstValue("FirstName")?.Trim(),
                User.FindFirstValue("MiddleName")?.Trim(),
                User.FindFirstValue("LastName")?.Trim()
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            if (string.IsNullOrWhiteSpace(evaluatorFullName))
            {
                evaluatorFullName = User.Identity?.Name
                    ?? User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue(ClaimTypes.Email)
                    ?? "Evaluator";
            }

            _unitOfWork.Register.UpdateEvaluator(register, evaluatorFullName);
            _unitOfWork.Save();
            try
            {
                return Json(new { success = true, message = "Evaluation updated successfully" });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = Convert.ToString(ex) });
            }
        }
        #endregion API CALLS

    }
}

