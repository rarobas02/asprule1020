using asprule1020.DataAccess.Documents.Certificate;
using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Infrastructure.Documents.Report;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO.Compression;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EvaluatorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly Rule1020Monitoring _rule1020Monitoring;

        public EvaluatorController(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            Rule1020Monitoring rule1020Monitoring)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _emailSender = emailSender;
            _rule1020Monitoring = rule1020Monitoring;
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
        public IActionResult GenerateCertificate(Guid? id)
        {
            if (!id.HasValue || id.Value == Guid.Empty)
            {
                return NotFound();
            }

            Register? registerFromDb = _unitOfWork.Register.Get(u => u.Id == id.Value);
            if (registerFromDb is null)
            {
                return NotFound();
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var certificateTemplate = new Rule1020Certificate(_webHostEnvironment, registerFromDb);

            var document = Document.Create(container =>
            {
                certificateTemplate.Compose(container);
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);

            return File(stream.ToArray(), "application/pdf", "Rule1020Certificate.pdf");
        }

        private static string BuildManagerName(string? first, string? middle, string? last)
        {
            return string.Join(" ", new[] { first, middle, last }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
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
        [Authorize(Roles = SD.Role_Evaluator)]
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
        [Authorize(Roles = SD.Role_Evaluator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendApprovedEmail(Guid id, string? est_email_message, IFormFile? est_certificate)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid record id.");
            }

            var register = _unitOfWork.Register.Get(u => u.Id == id);
            if (register is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(register.Email))
            {
                return BadRequest("No recipient email found.");
            }

            if (est_certificate is null || est_certificate.Length == 0)
            {
                return BadRequest("Certificate PDF is required.");
            }

            var extension = Path.GetExtension(est_certificate.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF certificate attachment is allowed.");
            }

            if (_emailSender is not EmailSender attachmentSender)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Email sender service is not configured for attachments.");
            }

            byte[] certificateBytes;
            await using (var ms = new MemoryStream())
            {
                await est_certificate.CopyToAsync(ms);
                certificateBytes = ms.ToArray();
            }

            var userProvince = User.FindFirstValue("EstProvince");
            var provinceInfo = ProvinceDetails.GetProvinceInfo(userProvince);

            var trimmedMessage = (est_email_message ?? string.Empty).Trim();
            var managerFullName = BuildManagerName(register.EstOwnerFirst, register.EstOwnerMid, register.EstOwnerLast);

            var body = $"""
                <div>
                    <p>Dear Establishment Owner / Manager {managerFullName},</p>

                    <p>Good day!</p>

                    <p>Upon evaluation of the submitted documents, we noted that your Rule 1020 Application is <strong>APPROVED</strong>.</p>

                    <p>You can Track your application using your Application Number: <strong>{register.TransId}</strong></p>

                    <p>You may also contact the provincial office landline {provinceInfo.ProvinceTelNo} or send an email to {provinceInfo.ProvinceEmail}</p>

                    <p>You may also visit {provinceInfo.ProvincialOffice} - {provinceInfo.ProvinceAddress} for inquiries and claim your certificate.</p>

                    <p>{provinceInfo.ProvincialOffice} remarks: <strong>{(string.IsNullOrWhiteSpace(trimmedMessage) ? "N/A" : System.Net.WebUtility.HtmlEncode(trimmedMessage))}</strong></p>

                    <p>Please do not reply on this email.</p>

                    <p>Thank you,<br>DOLE Regional Office 4a : {provinceInfo.ProvincialOffice}</p>
                </div>
                """;

            const string emailSubject = "DOLE Rule 1020 - Approved";
            var attachmentFileName = string.IsNullOrWhiteSpace(est_certificate.FileName) ? "Rule1020-Certificate.pdf" : Path.GetFileName(est_certificate.FileName);

            await attachmentSender.SendEmailAsync(
                register.Email,
                emailSubject,
                body,
                certificateBytes,
                attachmentFileName,
                "application/pdf");

            register.EstIsEmailApprovedSent = true;
            register.EstEmailApprovedSentDate = DateTime.Now;
            register.EstEvalEmailSendStatus = emailSubject;
            _unitOfWork.Save();

            TempData["success"] = "Approved email sent successfully.";
            return RedirectToAction(nameof(ApprovedItem), new { id });
        }
        [HttpGet]
        public IActionResult GenerateRule1020Monitoring(DateTime fromDate, DateTime toDate, string status)
        {
            var province = User.FindFirstValue("EstProvince");

            var items = _unitOfWork.Register
                .GetAll(r =>
                    r.EstProvince == province &&
                    (status == "All" || r.EstStatus == status) &&
                    r.EstRegistrationDate >= fromDate.Date &&
                    r.EstRegistrationDate <= toDate)
                .OrderByDescending(r => r.EstRegistrationDate)
                .ToList();

            var registerIds = items.Select(r => r.Id).ToHashSet();

            var emailByRegisterId = _userManager.Users
                .Where(u => u.RegisterId.HasValue && registerIds.Contains(u.RegisterId.Value))
                .Select(u => new { RegisterId = u.RegisterId!.Value, u.Email })
                .ToDictionary(x => x.RegisterId, x => x.Email ?? string.Empty);

            foreach (var item in items)
            {
                if (emailByRegisterId.TryGetValue(item.Id, out var email))
                {
                    item.Email = email;
                }
            }

            var bytes = _rule1020Monitoring.BuildMonitoringWorkbook(items);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rule-1020-report-{province?.ToUpper()}.xlsx");
        }


        #endregion API CALLS
    }
}

