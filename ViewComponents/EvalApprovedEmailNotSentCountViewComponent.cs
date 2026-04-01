using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace asprule1020.ViewComponents
{
    public class EvalApprovedEmailNotSentCountViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public EvalApprovedEmailNotSentCountViewComponent(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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
                if (HttpContext.Session.GetInt32(SD.EvalApprovedEmailNotSentCount) == null)
                {
                    HttpContext.Session.SetInt32(SD.EvalApprovedEmailNotSentCount, _unitOfWork.Register.GetAll(u => u.EstProvince == userProvince && u.EstIsEmailApprovedSent == false && u.EstStatus == SD.StatusApproved).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.EvalApprovedEmailNotSentCount));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}