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
            UserVM userVM = new()
            {
                UserRoleList = BuildRoleSelectList(),
                ApplicationUser = new ApplicationUser()
            };
            if (id != null)
            {
                var existingUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
                if (existingUser == null)
                {
                    return NotFound();
                }
                userVM.ApplicationUser = existingUser;
                userVM.SelectedRoleId = _db.UserRoles.Where(r => r.UserId == id).Select(r => r.RoleId).FirstOrDefault();
            }
            return View(userVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM userVM)
        {
            userVM.UserRoleList = BuildRoleSelectList();

            if (userVM.ApplicationUser == null)
            {
                ModelState.AddModelError(string.Empty, "User details are required.");
                return View(userVM);
            }

            if (!ModelState.IsValid)
            {
                return View(userVM);
            }

            var createResult = await _userManager.CreateAsync(userVM.ApplicationUser, userVM.Password!);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(userVM);
            }
            if (!string.IsNullOrEmpty(userVM.SelectedRoleId))
            {
                var roleName = _db.Roles
                    .Where(r => r.Id == userVM.SelectedRoleId)
                    .Select(r => r.Name)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(roleName))
                {
                    await _userManager.AddToRoleAsync(userVM.ApplicationUser, roleName);
                }
            }

            TempData["success"] = "User created successfully.";
            return RedirectToAction(nameof(ManageUser));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAllUser()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; //get the role ids
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return Json(new { data = objUserList });
        }
        [HttpPost]
        public IActionResult CreateApi([FromBody] ApplicationUser obj)
        {
            if (ModelState.IsValid)
            {
                _db.ApplicationUsers.Add(obj);
                _db.SaveChanges();
                return Json(new { success = true, message = "Create Successful" });
            }
            return Json(new { success = false, message = "Error while creating" });
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
                //user is currently locked, we will unlock them
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
        public IActionResult Delete(int? id)
        {
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
