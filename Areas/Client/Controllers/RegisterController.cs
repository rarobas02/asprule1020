using asprule1020.DataAccess.Repository.IRepository;
using asprule1020.Models.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace asprule1020.Areas.Client.Controllers
{
    [Area("Client")]
    public class RegisterController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult New(RegisterVM registerVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string FileUpdloadPath = Path.Combine(_webHostEnvironment.ContentRootPath,"Uploads");
                
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString(); 
                    var extension = Path.GetExtension(file.FileName);
                    var isPdf = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
                    if (!isPdf)
                    {
                        ModelState.AddModelError(nameof(file), "Only PDF files are allowed.");

                        return View();
                    }
                    string productPath = Path.Combine(FileUpdloadPath, @"sec_dti");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream); //upload the new image
                    }

                    if (registerVM.Register != null)
                    {
                        registerVM.Register.estSECFile = @"\sec_dti" + fileName + extension;
                    }
                }
                if (registerVM.Register != null)
                {
                    _unitOfWork.Register.Add(registerVM.Register);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
    }
}
