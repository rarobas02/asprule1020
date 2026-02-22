using asprule1020.DataAccess.Data;
using asprule1020.DataAccess.Repository;
using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private IEnumerable<SelectListItem> BuildRoleSelectList()
        {
            return _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }).ToList();
        }
        //[Authorize(SD.Role_Admin)]
        public IActionResult ManageUser()
        {
            return View();
        }
        public IActionResult Create(string? id)
        {
            var userVM = new UserVM
            {
                UserRoleList = BuildRoleSelectList(),
                ApplicationUser = new ApplicationUser { Id = string.Empty }
            };

            if (!string.IsNullOrEmpty(id))
            {
                var existingUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
                if (existingUser == null) { return NotFound(); }

                userVM.ApplicationUser = existingUser;
                userVM.SelectedRoleId = _db.UserRoles
                    .Where(r => r.UserId == id)
                    .Select(r => r.RoleId)
                    .FirstOrDefault();
            }

            return View(userVM);
        }

        #region API CALLS

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM? userVM)
        {
            const string idKey = $"{nameof(UserVM.ApplicationUser)}.{nameof(ApplicationUser.Id)}";

            userVM.UserRoleList = BuildRoleSelectList();
            if (userVM.ApplicationUser == null)
            {
                ModelState.AddModelError(string.Empty, "User details are required.");
                return View(userVM);
            }

            var isNewUser = string.IsNullOrWhiteSpace(userVM.ApplicationUser.Id);


            if (!isNewUser)
            {
                ModelState.Remove(nameof(UserVM.Password));
            }
            else if (isNewUser)
            {
                ModelState.Remove(idKey);
                ModelState.Remove(nameof(ApplicationUser.Id));
            }
            else if (string.IsNullOrWhiteSpace(userVM.Password))
            {
                ModelState.AddModelError(nameof(UserVM.Password), "Password is required for new users.");
            }

            if (!ModelState.IsValid)
            {
                return View(userVM);
            }

            ApplicationUser persistedUser;

            if (isNewUser)
            {
                userVM.ApplicationUser.Id = Guid.NewGuid().ToString();
                var createResult = await _userManager.CreateAsync(userVM.ApplicationUser, userVM.Password!);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.Remove(idKey);
                        ModelState.Remove(nameof(ApplicationUser.Id));
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(userVM);
                }

                persistedUser = userVM.ApplicationUser;
            }
            else
            {
                persistedUser = await _userManager.FindByIdAsync(userVM.ApplicationUser.Id);
                if (persistedUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to locate the user you are trying to update.");
                    return View(userVM);
                }

                persistedUser.FirstName = userVM.ApplicationUser.FirstName;
                persistedUser.MiddleName = userVM.ApplicationUser.MiddleName;
                persistedUser.LastName = userVM.ApplicationUser.LastName;
                persistedUser.Email = userVM.ApplicationUser.Email;
                persistedUser.UserName = userVM.ApplicationUser.UserName;
                persistedUser.PhoneNumber = userVM.ApplicationUser.PhoneNumber;
                persistedUser.EstProvince = userVM.ApplicationUser.EstProvince;

                var updateResult = await _userManager.UpdateAsync(persistedUser);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(userVM);
                }
            }

            await updateIdentityArtifactsAsync(persistedUser.Id, userVM.SelectedRoleId);

            TempData["success"] = isNewUser ? "User created successfully." : "User updated successfully.";
            return RedirectToAction(nameof(ManageUser));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DeletePost(string? id) => DeleteInternalAsync(id);

        [HttpGet]
        public IActionResult GetAllUser()
        {
            var objUserList = _db.ApplicationUsers.ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;
                user.Role = roleId == null
                    ? string.Empty
                    : roles.FirstOrDefault(u => u.Id == roleId)?.Name ?? string.Empty;
            }

            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Delete(string? id) => DeleteInternalAsync(id);

        private async Task<IActionResult> DeleteInternalAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { success = false, message = "User id is required." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            await RemoveUserIdentityArtifactsAsync(id);   // <-- clears AspNetUserRoles, etc.

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(" ", deleteResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }

            return Json(new { success = true, message = "User deleted successfully." });
        }

        private async Task RemoveUserIdentityArtifactsAsync(string userId)
        {
            var userRoles  = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            var userClaims = await _db.UserClaims.Where(uc => uc.UserId == userId).ToListAsync();
            var userLogins = await _db.UserLogins.Where(ul => ul.UserId == userId).ToListAsync();
            var userTokens = await _db.UserTokens.Where(ut => ut.UserId == userId).ToListAsync();

            if (userRoles.Count == 0 && userClaims.Count == 0 && userLogins.Count == 0 && userTokens.Count == 0)
                return;

            _db.UserRoles.RemoveRange(userRoles);
            _db.UserClaims.RemoveRange(userClaims);
            _db.UserLogins.RemoveRange(userLogins);
            _db.UserTokens.RemoveRange(userTokens);

            await _db.SaveChangesAsync();
        }
        private async Task updateIdentityArtifactsAsync(string userId, string? targetRoleId)
        {
            var userRoles = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();

            if (string.IsNullOrEmpty(targetRoleId))
            {
                if (userRoles.Count > 0)
                {
                    _db.UserRoles.RemoveRange(userRoles);
                    await _db.SaveChangesAsync();
                }
                return;
            }

            if (userRoles.Count == 1 && userRoles[0].RoleId == targetRoleId)
            {
                return;
            }

            _db.UserRoles.RemoveRange(userRoles);
            _db.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = targetRoleId
            });

            await _db.SaveChangesAsync();
        }
        #endregion

    }
}
