using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace asprule1020.ViewComponents
{
    public class PoHeadForReviewCountViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public PoHeadForReviewCountViewComponent(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userProvince = HttpContext.User.FindFirstValue("EstProvince");
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            if (claim is not null)
            {
                if (HttpContext.Session.GetInt32(SD.PoHeadForReviewCount) == null)
                {
                    HttpContext.Session.SetInt32(SD.PoHeadForReviewCount, _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && (u.EstStatus == SD.StatusForApproval || u.EstStatus == SD.StatusForReapplication)).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.PoHeadForReviewCount));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}