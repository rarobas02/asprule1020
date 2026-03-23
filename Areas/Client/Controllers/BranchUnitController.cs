using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = SD.Role_Client)]
    public class BranchUnitController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BranchUnitController(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetBranchUnit(Guid id)
        {
            var branchUnits = _unitOfWork.BranchUnit
                .GetAll(r => r.RegisterId == id)
                .ToList();

            return Json(new { success = true, data = branchUnits });
        }

        [HttpPost, ActionName("DeleteBranchUnit")]
        public IActionResult DeletePOST(Guid? id)
        {
            BranchUnit? obj = _unitOfWork.BranchUnit.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.BranchUnit.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Branch Unit deleted successfully." });
        }
        #endregion API CALLS
    }
}
