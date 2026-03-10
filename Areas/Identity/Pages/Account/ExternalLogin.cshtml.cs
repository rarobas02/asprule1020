// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using asprule1020.DataAccess.Data;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace asprule1020.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            public Register Register { get; set; }
            public IFormFile EstSECFileUpload { get; set; }
            public IFormFile EstBisPermitFileUpload { get; set; }
            public IFormFile EstOwnerValidIdFileUpload { get; set; }
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user is null && info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email)?.Trim();
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        user = await _userManager.FindByEmailAsync(email);
                    }
                }

                if (user is null)
                {
                    return LocalRedirect(GetSafeReturnUrl(returnUrl));
                }

                return await RedirectByRoleAsync(user, returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            if (result.IsNotAllowed)
            {
                ErrorMessage = "Please confirm your email address before logging in.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // If external login is not linked yet, try linking to an existing account by email first.
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)?.Trim();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(existingUser, isPersistent: false, info.LoginProvider);
                            _logger.LogInformation("Linked {LoginProvider} login to existing user {Email}.", info.LoginProvider, email);
                            return await RedirectByRoleAsync(existingUser, returnUrl);
                        }
                    }
                }
            }

            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;

            if (!await _roleManager.RoleExistsAsync(SD.Role_Client))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Client));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Evaluator));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Po_Head));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Region_Focal));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
            }

            EnsureInputInitialized();
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            EnsureInputInitialized();
            PopulateTransId();
            NormalizeConditionalFields();
            RemoveGeneratedFileFieldsFromModelState();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.EstProvince = Input.Register?.EstProvince?.Trim();
                user.FirstName = Input.Register?.EstOwnerFirst?.Trim();
                user.LastName = Input.Register?.EstOwnerLast?.Trim();
                user.MiddleName = Input.Register?.EstOwnerMid?.Trim();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Client);

                        var registerEntity = Input.Register!;
                        registerEntity.TransId ??= $"TR-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
                        registerEntity.UserName = registerEntity.UserName ?? Input.Email;
                        registerEntity.EstSECFile = await SavePdfAsync(Input.EstSECFileUpload, "sec_dti", "-sec");
                        registerEntity.EstBisPermitFile = await SavePdfAsync(Input.EstBisPermitFileUpload, "bus_perm", "-bus_permit");
                        registerEntity.EstOwnerValidIDFile = await SavePdfAsync(Input.EstOwnerValidIdFileUpload, "valid_id", "-validid");

                        _db.Registers.Add(registerEntity);
                        await _db.SaveChangesAsync();

                        user.RegisterId = registerEntity.Id;
                        await _userManager.UpdateAsync(user);

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return await RedirectByRoleAsync(user, returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
        //CUSTOM BACKEND CODE BELOW
        private void EnsureInputInitialized()
        {
            Input ??= new InputModel();
            Input.Register ??= new Register();
            Input.Register.EstTechInfo1 ??= new List<string>();
            Input.Register.EstTechInfo2 ??= new List<string>();
        }
        private async Task<string> SavePdfAsync(IFormFile file, string subFolder, string suffix)
        {
            if (file is null || file.Length == 0)
            {
                return string.Empty;
            }

            var uploadsRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", subFolder);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}{suffix}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;                    // <-- only the file name
            // return Path.Combine(Path.DirectorySeparatorChar + subFolder, fileName); // <-- if you prefer relative path
        }
        private void PopulateTransId()
        {
            Input.Register.TransId ??= $"RO4A-1020-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            ModelState.Remove("Input.Register.TransId");
        }

        private void NormalizeConditionalFields()
        {
            if (!string.Equals(Input.Register.EstBusinessNature, "Others,Please Specify", StringComparison.OrdinalIgnoreCase))
            {
                Input.Register.EstOtherBusNature = null;
                ModelState.Remove("Input.Register.EstOtherBusNature");
            }

            if (!(Input.Register.EstTechInfo1?.Contains("Others") ?? false))
            {
                Input.Register.EstTechInfo1Other = null;
                ModelState.Remove("Input.Register.EstTechInfo1Other");
            }

            if (!(Input.Register.EstTechInfo2?.Contains("Others") ?? false))
            {
                Input.Register.EstTechInfo2Other = null;
                ModelState.Remove("Input.Register.EstTechInfo2Other");
            }
        }

        private void RemoveGeneratedFileFieldsFromModelState()
        {
            ModelState.Remove("Input.Register.EstSECFile");
            ModelState.Remove("Input.Register.EstBisPermitFile");
            ModelState.Remove("Input.Register.EstOwnerValidIDFile");
        }

        private string GetSafeReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;

            return Url.Content("~/");
        }

        private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser user, string returnUrl)
        {
            var registerId = user.RegisterId?.ToString();

            if (await _userManager.IsInRoleAsync(user, SD.Role_Client))
            {
                if (!user.RegisterId.HasValue)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Your account is not linked to any registration yet.");
                    return Page();
                }

                var register = await _db.Registers.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == user.RegisterId.Value);

                if (register is null)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Unable to locate your registration record. Please contact support.");
                    return Page();
                }

                if (!string.Equals(register.EstStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    await _signInManager.SignOutAsync();
                    var statusLabel = string.IsNullOrWhiteSpace(register.EstStatus) ? "review" : register.EstStatus;
                    ModelState.AddModelError(string.Empty, $"Your Registration Status is still {statusLabel}, only Approved Application is allowed to update. Create new Registration For Re-application");
                    return Page();
                }

                return RedirectToAction("Index", "Update", new { area = "Client", registerId });
            }

            if (await _userManager.IsInRoleAsync(user, SD.Role_Evaluator))
                return RedirectToAction("Review", "Evaluator", new { area = "Admin" });

            if (await _userManager.IsInRoleAsync(user, SD.Role_Po_Head))
                return RedirectToAction("PoHeadReview", "PoHead", new { area = "Admin" });

            if (await _userManager.IsInRoleAsync(user, SD.Role_Region_Focal))
                return RedirectToAction("RegionReview", "User", new { area = "Admin" });

            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
                return RedirectToAction("ManageUser", "User", new { area = "Admin" });

            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }
    }
}
