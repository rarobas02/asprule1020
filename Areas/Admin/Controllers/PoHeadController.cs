using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Globalization;
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
        private static readonly HashSet<string> R4aProvinces = new(StringComparer.OrdinalIgnoreCase)
        {
            "Laguna",
            "Cavite",
            "Batangas",
            "Rizal",
            "Quezon"
        };
        //TODO: Refactor the API calls to a single method with a parameter for status to avoid code duplication
        public IActionResult ReviewItem(Guid? id)
        {
            var province = User.FindFirstValue("EstProvince");
            if (id == null || id == Guid.Empty)
            {
                return NotFound();
            }
            var registerVM = BuildRegisterVm(id.Value);
            if (registerVM is null)
            {
                return NotFound();
            }
            var EvalApprovedEmailNotSentCount = _unitOfWork.Register
.GetAll(u => u.EstProvince == province && (u.EstStatus == SD.StatusForApproval || u.EstStatus == SD.StatusForReapplication))
.Count();

            HttpContext.Session.SetInt32(SD.EvalApprovedEmailNotSentCount, EvalApprovedEmailNotSentCount);
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
            var registerVM = BuildRegisterVm(id.Value);
            if (registerVM is null)
            {
                return NotFound();
            }

            return View(registerVM);
        }

        private RegisterVM? BuildRegisterVm(Guid id)
        {
            var register = _unitOfWork.Register.Get(u => u.Id == id);
            if (register is null)
            {
                return null;
            }

            return new RegisterVM
            {
                Register = register,
                CheckList = _unitOfWork.EvaluationChecklist.Get(x => x.RegisterId == id)
                    ?? new EvaluationChecklist { RegisterId = id },
                Remarks = _unitOfWork.EvaluationRemark.Get(x => x.RegisterId == id)
                    ?? new EvaluationRemark { RegisterId = id }
            };
        }

        private string? BuildRule1020Number(string? estProvince)
        {
            if (string.IsNullOrWhiteSpace(estProvince))
            {
                return null;
            }

            if (!R4aProvinces.Contains(estProvince))
            {
                return null;
            }

            var rowCount = _unitOfWork.Register.GetAll(r => r.EstProvince == estProvince && r.EstStatus == SD.StatusApproved).Count(); // count all approved records from the user province
            var lastId = rowCount + 1;

            var provinceInitial = estProvince[..1].ToUpper(CultureInfo.InvariantCulture);
            return $"RO4A-1020-{provinceInitial}PO-{DateTime.Now:MMyy-}{lastId:0000}";
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
            var userProvince = HttpContext.User.FindFirstValue("EstProvince");
            if (register.Id == Guid.Empty)
            {
                return BadRequest("Invalid register id.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var evaluator = await _userManager.FindByIdAsync(userId!);
            var evaluatorFullName = string.Join(" ", new[]
            {
        evaluator?.FirstName?.Trim(),
        evaluator?.MiddleName?.Trim(),
        evaluator?.LastName?.Trim()
    }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var registerFromDb = _unitOfWork.Register.Get(u => u.Id == register.Id);
            if (registerFromDb is null)
            {
                return NotFound("Register record not found.");
            }

            var province = registerFromDb.EstProvince; // <-- province value by Id
            var rule1020Id = BuildRule1020Number(province);

            if (string.IsNullOrWhiteSpace(rule1020Id))
            {
                return BadRequest("Invalid province.");
            }

            _unitOfWork.Register.UpdatePoHead(register, evaluatorFullName, rule1020Id);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.PoHeadApprovedCount, _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && u.EstStatus == SD.StatusForApproval).Count());
            HttpContext.Session.SetInt32(SD.PoHeadForReviewCount, _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && ((u.EstStatus == SD.StatusForApproval) || (u.EstStatus == SD.StatusForReapplication))).Count());
            return Json(new
            {
                success = true,
                message = "Evaluation updated successfully",
                trans_no = registerFromDb.TransId,
                recommendation = register.EstStatus
            });
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
