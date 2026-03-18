// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using asprule1020.DataAccess.Data;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace asprule1020.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;


        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 
        private void EnsureInputInitialized()
        {
            Input ??= new InputModel();
            Input.Register ??= new Register();
            Input.Register.EstTechInfo1 ??= new List<string>();
            Input.Register.EstTechInfo2 ??= new List<string>();
        }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public Register Register { get; set; }
            public List<BranchUnitInput> BranchUnits { get; set; } = [];
            public List<LaborUnionInput> LaborUnions { get; set; } = [];
            public IFormFile EstSECFileUpload { get; set; }
            public IFormFile EstBisPermitFileUpload { get; set; }
            public IFormFile EstOwnerValidIdFileUpload { get; set; }
        }
        public class BranchUnitInput
        {
            public string? Rule1020Number { get; set; }
            public string? BranchName { get; set; }
            public string? BranchAddress { get; set; }
        }

        public class LaborUnionInput
        {
            public string? UnionName { get; set; }
            public string? UnionAddress { get; set; }
            public string? UnionBLR { get; set; }
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            EnsureInputInitialized();
            PopulateTransId();
            NormalizeConditionalFields();
            RemoveGeneratedFileFieldsFromModelState();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = CreateUser();
            user.EstProvince = Input.Register.EstProvince?.Trim();
            user.FirstName = Input.Register?.EstOwnerFirst?.Trim();
            user.LastName = Input.Register?.EstOwnerLast?.Trim();
            user.MiddleName = Input.Register?.EstOwnerMid?.Trim();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            user.Email = Input.Email;
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                await _userManager.AddToRoleAsync(user, SD.Role_Client); // Assign the "Client" role to the newly registered user
                var registerEntity = Input.Register!;
                registerEntity.TransId ??= $"RO4A-1020-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
                registerEntity.UserName = registerEntity.UserName ?? Input.Email;
                registerEntity.Email = Input.Email;
                registerEntity.EstSECFile = await SavePdfAsync(Input.EstSECFileUpload, "sec_dti", "-sec");
                registerEntity.EstBisPermitFile  = await SavePdfAsync(Input.EstBisPermitFileUpload, "bus_perm", "-bus_permit");
                registerEntity.EstOwnerValidIDFile = await SavePdfAsync(Input.EstOwnerValidIdFileUpload, "valid_id", "-validid");

                _db.Registers.Add(registerEntity);
                await _db.SaveChangesAsync();

                var branchEntities = Input.BranchUnits
                    .Where(branchUnit => !string.IsNullOrWhiteSpace(branchUnit.Rule1020Number)
                             || !string.IsNullOrWhiteSpace(branchUnit.BranchName)
                             || !string.IsNullOrWhiteSpace(branchUnit.BranchAddress))
                    .Select(branchUnit => new BranchUnit
                    {
                        RegisterId = registerEntity.Id,
                        Rule1020Number = branchUnit.Rule1020Number?.Trim(),
                        BranchName = branchUnit.BranchName?.Trim(),
                        BranchAddress = branchUnit.BranchAddress?.Trim()
                    })
                    .ToList();

                if (branchEntities.Count > 0)
                {
                    _db.BranchUnits.AddRange(branchEntities);
                }

                var laborUnionEntities = Input.LaborUnions
                    .Where(laborUnion => !string.IsNullOrWhiteSpace(laborUnion.UnionName)
                             || !string.IsNullOrWhiteSpace(laborUnion.UnionAddress)
                             || !string.IsNullOrWhiteSpace(laborUnion.UnionBLR))
                    .Select(laborUnion => new LaborUnion
                    {
                        RegisterId = registerEntity.Id,
                        UnionName = laborUnion.UnionName?.Trim(),
                        UnionAddress = laborUnion.UnionAddress?.Trim(),
                        UnionBLR = laborUnion.UnionBLR?.Trim()
                    })
                    .ToList();

                if (laborUnionEntities.Count > 0)
                {
                    _db.LaborUnions.AddRange(laborUnionEntities);
                }

                await _db.SaveChangesAsync();

                user.RegisterId = registerEntity.Id;
                await _userManager.UpdateAsync(user);

                // Redirect to Client confirmation page with submitted registration id.
                var confirmationUrl = Url.Action(
                    "RegisterConfirmation",
                    "Register",
                    new { area = "Client", id = registerEntity.Id });

                if (!string.IsNullOrWhiteSpace(confirmationUrl))
                {
                    return LocalRedirect(confirmationUrl);
                }

                // Fallback (should rarely happen)
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Ensure roles exist - if not exist, the condition will create the USER roles
            if (!_roleManager.RoleExistsAsync(SD.Role_Client).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Client)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Evaluator)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Po_Head)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Region_Focal)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            }
            ReturnUrl = returnUrl;
            EnsureInputInitialized();
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
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
    }
}
