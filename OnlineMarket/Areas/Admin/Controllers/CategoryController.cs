using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using OnlineMarket.Utility;
using System.Threading.Tasks;

namespace OnlineMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/Category
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if (id == null)
            {
                // This code for create
                return View(category);
            }
            // This code for edit
            category = await _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            return View(category);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category item)
        {
            if (ModelState.IsValid)
            {
                if(item.Id == 0)
                {
                    await _unitOfWork.Category.Add(item);
                }
                else
                {
                    await _unitOfWork.Category.UpdateAsync(item);
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
            var allObj = _unitOfWork.Category.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _unitOfWork.Category.Get(id);

            if(model == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.Category.Remove(model);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "The object has been deleted" });
        }

        #endregion
    }
}
