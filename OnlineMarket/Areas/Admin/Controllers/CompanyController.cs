using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using OnlineMarket.Utility;
using System.Threading.Tasks;

namespace OnlineMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/Company
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Company item = new Company();
            if (id == null)
            {
                // This code for create
                return View(item);
            }
            // This code for edit
            item = await _unitOfWork.Company.Get(id.GetValueOrDefault());
            if (item == null)
            {
                return NotFound();
            }
            return View(item);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Company item)
        {
            if (ModelState.IsValid)
            {
                if (item.Id == 0)
                {
                    await _unitOfWork.Company.Add(item);
                }
                else
                {
                    _unitOfWork.Company.Update(item);
                }
                await _unitOfWork.SaveAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Company.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _unitOfWork.Company.Get(id);

            if (model == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.Company.Remove(model);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "The object has been deleted" });
        }

        #endregion
    }
}
