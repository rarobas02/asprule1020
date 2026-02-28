using asprule1020.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace asprule1020.ViewComponents
{
    public class UserFullNameViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserFullNameViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var parts = new[]
            {
                HttpContext.User.FindFirstValue("FirstName")?.Trim(),
                HttpContext.User.FindFirstValue("MiddleName")?.Trim(),
                HttpContext.User.FindFirstValue("LastName")?.Trim()
            };

            var displayName = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = HttpContext.User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? string.Empty;
            }

            return View("~/Areas/Admin/Views/Shared/Components/UserFullName/Default.cshtml", displayName);
        }
    }
}
