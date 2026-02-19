using asprule1020.DataAccess.Data;
using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace asprule1020.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        //[Authorize(SD.Role_Admin)]
        public IActionResult ManageUser()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAllUser()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach(var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; //get the role ids
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return Json(new { data = objUserList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
