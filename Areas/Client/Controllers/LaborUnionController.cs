using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models;
using asprule1020.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = SD.Role_Client)]
    public class LaborUnionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public LaborUnionController(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #region API CALLS
        [HttpPost]
        public IActionResult AddLaborUnion(Guid registerId, string UnionName, string UnionAddress, string UnionBLR)
        {
            try
            {
                if (registerId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid register id." });
                }

                if (string.IsNullOrWhiteSpace(UnionName) || string.IsNullOrWhiteSpace(UnionAddress))
                {
                    return Json(new { success = false, message = "Labor Union name and Labor Union BLR are required." });
                }

                var laborUnion = new LaborUnion
                {
                    Id = Guid.NewGuid(),
                    RegisterId = registerId,
                    UnionName = UnionName?.Trim(),
                    UnionAddress = UnionAddress.Trim(),
                    UnionBLR = UnionBLR.Trim()
                };

                _unitOfWork.LaborUnion.Add(laborUnion);
                _unitOfWork.Save();

                return Json(new
                {
                    success = true,
                    message = "Labor Union created successfully.",
                    data = laborUnion
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetLaborUnion(Guid id)
        {
            var laborUnion = _unitOfWork.LaborUnion
                .GetAll(r => r.RegisterId == id)
                .ToList();

            return Json(new { success = true, data = laborUnion });
        }
        [HttpPost]
        public IActionResult UpdateLaborUnion(Guid id, string UnionName, string UnionAddress, string UnionBLR)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid labor union id." });
                }

                if (string.IsNullOrWhiteSpace(UnionName) || string.IsNullOrWhiteSpace(UnionAddress)|| string.IsNullOrWhiteSpace(UnionBLR))
                {
                    return Json(new { success = false, message = "All Labor Union Inputs are required." });
                }

                var existing = _unitOfWork.LaborUnion.Get(u => u.Id == id);
                if (existing == null)
                {
                    return Json(new { success = false, message = "Branch unit not found." });
                }

                _unitOfWork.LaborUnion.UpdateLaborUnion(
                    id,
                    UnionName?.Trim() ?? string.Empty,
                    UnionAddress.Trim(),
                    UnionBLR.Trim());

                _unitOfWork.Save();

                return Json(new { success = true, message = "Branch Unit updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost, ActionName("DeleteLaborUnion")]
        public IActionResult DeletePOST(Guid? id)
        {
            LaborUnion? obj = _unitOfWork.LaborUnion.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.LaborUnion.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Labor Union deleted successfully." });
        }

        #endregion API CALLS
    }
}
