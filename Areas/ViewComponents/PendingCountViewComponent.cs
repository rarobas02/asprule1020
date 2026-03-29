using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace asprule1020.Areas.ViewComponents
{
    public class PendingCountViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public PendingCountViewComponent(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userProvince = HttpContext.User.FindFirstValue("EstProvince") ?? string.Empty;
            var pendingCount = HttpContext.Session.GetInt32(SD.PendingCount) ?? 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            if (claim is null)
            {
                pendingCount = _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && u.EstIsEmailApprovedSent == false).Count();

                if (HttpContext.Session.GetInt32(SD.PendingCount) == null)
                {
                    HttpContext.Session.SetInt32(SD.PendingCount, _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && u.EstIsEmailApprovedSent == false).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.PendingCount));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}