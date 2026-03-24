using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Models.ViewModel;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

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
        [HttpPost]
        public IActionResult AddBranchUnit(Guid registerId, string rule1020Number, string branchName, string branchAddress)
        {
            try
            {
                if (registerId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid register id." });
                }

                if (string.IsNullOrWhiteSpace(branchName) || string.IsNullOrWhiteSpace(branchAddress))
                {
                    return Json(new { success = false, message = "Branch name and branch address are required." });
                }

                var branchUnit = new BranchUnit
                {
                    Id = Guid.NewGuid(),
                    RegisterId = registerId,
                    Rule1020Number = rule1020Number?.Trim(),
                    BranchName = branchName.Trim(),
                    BranchAddress = branchAddress.Trim()
                };

                _unitOfWork.BranchUnit.Add(branchUnit);
                _unitOfWork.Save();

                return Json(new
                {
                    success = true,
                    message = "Branch Unit created successfully.",
                    data = branchUnit
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetBranchUnit(Guid id)
        {
            var branchUnits = _unitOfWork.BranchUnit
                .GetAll(r => r.RegisterId == id)
                .ToList();

            return Json(new { success = true, data = branchUnits });
        }
        [HttpPost]
        public IActionResult UpdateBranchUnit(Guid id, string rule1020Number, string branchName, string branchAddress)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid branch id." });
                }

                if (string.IsNullOrWhiteSpace(branchName) || string.IsNullOrWhiteSpace(branchAddress))
                {
                    return Json(new { success = false, message = "Branch name and branch address are required." });
                }

                var existing = _unitOfWork.BranchUnit.Get(u => u.Id == id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "Branch unit not found." });
                }

                _unitOfWork.BranchUnit.UpdateBranchUnit(
                    id,
                    rule1020Number?.Trim() ?? string.Empty,
                    branchName.Trim(),
                    branchAddress.Trim());

                _unitOfWork.Save();

                return Json(new { success = true, message = "Branch Unit updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
