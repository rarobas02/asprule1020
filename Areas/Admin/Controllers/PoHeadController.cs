using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Po_Head)]
    public class PoHeadController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public PoHeadController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        [Area("Admin")]
        public IActionResult PoHeadReview()
        {
            return View();
        }
        //TODO: Refactor the API calls to a single method with a parameter for status to avoid code duplication
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
        #region API CALLS
        [HttpGet]
        public IActionResult GetAllForApprovalAndReapply(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && (u.EstStatus == SD.StatusForApproval || u.EstStatus == SD.StatusForReapplication)).ToList();
            return Json(new { data = objRegisterList });
        }
        [HttpGet]
        public IActionResult GetAllApproved(string status)
        {
            var province = User.FindFirstValue("EstProvince");
            List<Register> objRegisterList = _unitOfWork.Register.GetAll(u => u.EstProvince == province && u.EstStatus == SD.StatusApproved).ToList();
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

            _unitOfWork.Register.UpdatePoHead(register, evaluatorFullName);
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
        [HttpGet]
        public IActionResult ViewAttachment(Guid id, string type)
        {
            if (id == Guid.Empty || string.IsNullOrWhiteSpace(type))
            {
                return NotFound();
            }

            var register = _unitOfWork.Register.Get(u => u.Id == id);
            if (register is null)
            {
                return NotFound();
            }

            var key = type.Trim().ToLowerInvariant();
            var (folder, storedPath) = key switch
            {
                "sec_dti" => ("sec_dti", register.EstSECFile),
                "bus_perm" => ("bus_perm", register.EstBisPermitFile),
                "valid_id" => ("valid_id", register.EstOwnerValidIDFile),
                _ => (string.Empty, string.Empty)
            };

            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(storedPath))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(storedPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", folder, fileName);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }
        #endregion API CALLS
    }
}
