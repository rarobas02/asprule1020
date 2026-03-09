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

        public UpdateController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
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
    }
}
