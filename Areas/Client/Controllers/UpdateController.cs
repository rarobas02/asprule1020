using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = SD.Role_Client)]
    public class UpdateController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UpdateController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(Guid? registerId)
        {
            if (registerId == null || registerId == Guid.Empty)
            {
                return NotFound();
            }

            var register = _unitOfWork.Register.Get(u => u.Id == registerId);
            if (register == null)
            {
                return NotFound();
            }

            var updateVM = new UpdateVM
            {
                Register = register,
                ApplicationUser = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.RegisterId == registerId)
            };

            return View(updateVM);
        }
        private async Task<string> UpdateFileAsync(
    IFormFile file,
    string subFolder,
    string? oldFileName,
    string suffix)
        {
            if (file is null || file.Length == 0)
            {
                return oldFileName ?? string.Empty;
            }

            var uploadsRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", subFolder);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = !string.IsNullOrWhiteSpace(oldFileName)
                ? oldFileName
                : $"{Guid.NewGuid()}{suffix}{Path.GetExtension(file.FileName)}";

            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            return fileName;
        }
        #region UPDATE API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateVM updateVM)
        {
            // the model state is always invalid because there are required tags in register model that is not present in the update form
            // TODO: bypass required from the db for update
            //if (!ModelState.IsValid || updateVM.Register == null || updateVM.Register.Id == Guid.Empty)
            //{
            //    return View(nameof(Index), updateVM);
            //}

            var existing = _unitOfWork.Register.Get(u => u.Id == updateVM.Register.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // default: keep old names
            updateVM.Register.EstSECFile = existing.EstSECFile;
            updateVM.Register.EstBisPermitFile = existing.EstBisPermitFile;
            updateVM.Register.EstOwnerValidIDFile = existing.EstOwnerValidIDFile;

            var secFile = Request.Form.Files.GetFile("Register.EstSECFile");
            var permitFile = Request.Form.Files.GetFile("Register.EstBisPermitFile");
            var validIdFile = Request.Form.Files.GetFile("Register.EstOwnerValidIDFile");

            if (secFile is { Length: > 0 })
                updateVM.Register.EstSECFile = await UpdateFileAsync(secFile, "sec_dti", existing.EstSECFile, "-sec");

            if (permitFile is { Length: > 0 })
                updateVM.Register.EstBisPermitFile = await UpdateFileAsync(permitFile, "bus_perm", existing.EstBisPermitFile, "-bus_permit");

            if (validIdFile is { Length: > 0 })
                updateVM.Register.EstOwnerValidIDFile = await UpdateFileAsync(validIdFile, "valid_id", existing.EstOwnerValidIDFile, "-validid");

            _unitOfWork.UpdateRegistration.UpdateRegistration(updateVM.Register);
            _unitOfWork.Save();

            TempData["success"] = "Establishment Registration Updated Successfully";
            return RedirectToAction(nameof(Index), new { registerId = updateVM.Register.Id });
        }

        #endregion

    }
}
