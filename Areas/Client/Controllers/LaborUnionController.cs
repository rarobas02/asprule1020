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
        public IActionResult AddLaborUnion(Guid Id)
        {
            var laborUnion = _unitOfWork.LaborUnion.Get(r => r.Id == Id);
            if (laborUnion is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(laborUnion);
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
        public IActionResult UpdateLaborUnion() // IN THE PARAMETERS, GET THE INPUTS FROM THE JAVASCRIPT
        {
            //LaborUnion? obj = _unitOfWork.LaborUnion.UpdateLaborUnion(PARAMETERS INPUTS SHOULD GO HERE);
 
            _unitOfWork.Save();
            try
            {
                return Json(new { success = true, message = "Labor Union successfully Updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = Convert.ToString(ex) });
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
