using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    public class RegisterController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserManager<ApplicationUser> _applicationUserManager;

        public RegisterController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager, UserManager<ApplicationUser> applicationUserManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _applicationUserManager = applicationUserManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(RegisterVM registerVM, IFormFile? secFile,IFormFile? bisPermitFile,IFormFile? validIdFile)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            string uploadRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads");

            if (registerVM.Register is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid form payload.");
                return View(registerVM);
            }

            registerVM.Register.EstSECFile = SavePdf(secFile, uploadRoot, "sec_dti");
            registerVM.Register.EstBisPermitFile = SavePdf(bisPermitFile, uploadRoot, "bus_perm");
            registerVM.Register.EstOwnerValidIDFile = SavePdf(validIdFile, uploadRoot, "valid_id");

            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            _unitOfWork.Register.Add(registerVM.Register);
            _unitOfWork.Save();

            TempData["success"] = "Establishment Registration is Under Review";
            return RedirectToAction(nameof(Index));
        }
        private string SavePdf(IFormFile? file, string rootPath, string subFolder)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError(subFolder, "File is required.");
                return string.Empty;
            }

            var extension = Path.GetExtension(file.FileName);
            bool isPdf = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
                         && string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

            if (!isPdf)
            {
                ModelState.AddModelError(subFolder, "Only PDF files are allowed.");
                return string.Empty;
            }

            string folderPath = Path.Combine(rootPath, subFolder);
            Directory.CreateDirectory(folderPath);

            string fileName = $"{Guid.NewGuid()}{extension}";
            string relativePath = Path.Combine(Path.DirectorySeparatorChar + subFolder, fileName);
            string absolutePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(absolutePath, FileMode.Create);
            file.CopyTo(stream);

            return relativePath;
        }
        public IActionResult GetProvDist(string? estRegion)
        {
            List<PhProvDist> objProvDistList = _unitOfWork.Province.GetAll(p => p.Region == estRegion).ToList();

            return Json(new { status = true, data = objProvDistList });
        }

        public IActionResult GetCityMun(string? estProvince)
        {
            List<PhCityMun> objCityMunList = _unitOfWork.CityMunicipality.GetAll(c => c.ProvinceDistrict == estProvince).ToList();
            return Json(new { status = true, data = objCityMunList });
        }

        public IActionResult GetBrgy(string? estCityMun)
        {
            List<PhBarangay> objBarangayList = _unitOfWork.Barangay.GetAll(b => b.CityMunicipality == estCityMun).ToList();

            return Json(new { status = true, data = objBarangayList });
        }
        public IActionResult IsUsernameExist(string userName)
        {
            var existingUser = _applicationUserManager.Users.FirstOrDefault(r => r.EstUsername == userName);
            if (existingUser is not null)
            {
                return Json(new { status = false, data = $"Username {userName} is already taken." });
            }
            return Json(new { status = true, data = true });
        }
        public IActionResult IsEmailExist(string estEmail)
        {
            var existingUser = _userManager.Users.FirstOrDefault(r => r.Email == estEmail);
            if (existingUser is not null)
            {
                return Json(new { status = false, data = $"Establishment Email: {estEmail} is already taken." });
            }
            return Json(new { status = true, data = true });
        }
    }
}
