using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class EvaluatorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public EvaluatorController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult Review()
        {
            return View();
        }
        [Authorize(Roles = SD.Role_Evaluator)]
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
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult Approved()
        {
            return View();
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult ApprovedItem(Guid? id)
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
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult UpdateItem(Guid? id)
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
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult Reapplication()
        {
            return View();
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        public IActionResult Updating()
        {
            return View();
        }
        public IActionResult ViewAll()
        {
            return View();
        }
        #region API CALLS

        //TODO: Refactor the API calls to a single method with a parameter for status to avoid code duplication
        [Authorize(Roles = SD.Role_Evaluator)]
        [HttpGet]
        public IActionResult GetAllForReview(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == SD.StatusForReview).ToList();
            return Json(new { data = objRegisterList });
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        [HttpGet]
        public IActionResult GetAllApproved(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == SD.StatusApproved).ToList();
            return Json(new { data = objRegisterList });
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        [HttpGet]
        public IActionResult GetAllForReapplication(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == SD.StatusForReapplication).ToList();
            return Json(new { data = objRegisterList });
        }
        [Authorize(Roles = SD.Role_Evaluator)]
        [HttpGet]
        public IActionResult GetAllForUpdate(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == SD.StatusForUpdate).ToList();
            return Json(new { data = objRegisterList });
        }
        [Authorize(Roles = $"{SD.Role_Evaluator},{SD.Role_Po_Head}")]
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register
                .GetAll(u => u.EstProvince == province)
                .ToList();
            return Json(new { data = objRegisterList });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluationResult(Register register)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var evaluator = await _userManager.FindByIdAsync(userId);
            var evaluatorFullName = string.Join(" ", new[]
            {
        evaluator?.FirstName?.Trim(),
        evaluator?.MiddleName?.Trim(),
        evaluator?.LastName?.Trim()
    }.Where(part => !string.IsNullOrWhiteSpace(part)));

            _unitOfWork.Register.UpdateEvaluator(register, evaluatorFullName);
            _unitOfWork.Save();
            try
            {
                return Json(new { success = true, message = "Evaluation updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = Convert.ToString(ex) });
            }
        }
    #endregion API CALLS

    }
}

