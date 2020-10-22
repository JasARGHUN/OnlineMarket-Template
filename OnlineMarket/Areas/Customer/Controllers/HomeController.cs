using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using OnlineMarket.Utility;
using ReflectionIT.Mvc.Paging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineMarket.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string filter, int page = 1)
        {
            //IEnumerable<Product> itemList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            // includeProperties: "Category" automatically include a Category in a Product
            var qry = _unitOfWork.Product.GetAll(includeProperties: "Category");

            if (!string.IsNullOrWhiteSpace(filter))
            {
                qry = qry.Where(p => p.Name.Contains(filter));
            }

            var itemList = PagingList.Create(qry, 3, page);
            
            itemList.RouteValue = new RouteValueDictionary {{ "filter", filter}};

            // РЕАЛИЗОВАТЬ ТУТ или в Index.cshtml: Перенаправлять на страницу EmptyPage если пойск ничего не нашел

            // Refresh Shopping Cart section begin.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim != null)
            {
                // Configure Sessions for Cart
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count();

                HttpContext.Session.SetInt32(SD.SessionShoppingCart, count);
            }

            // Refresh Shopping Cart section end.

            return View(itemList);
        }

        public IActionResult Details(int? id)
        {
            // includeProperties: "Category" automatically include a Category in a Product
            var model = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties: "Category");

            ShoppingCart cartModel = new ShoppingCart()
            {
                Product = model,
                ProductId = model.Id
            };

            return View(cartModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // ?
        public async Task<IActionResult> Details(ShoppingCart item)
        {
            item.Id = 0;

            if (ModelState.IsValid)
            {
                // Add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                item.ApplicationUserId = claim.Value;

                // includeProperties: "Product" automatically include a Product in a ShoppingCart
                ShoppingCart cartModel = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.ApplicationUserId == item.ApplicationUserId &&
                    x.ProductId == item.ProductId, includeProperties: "Product");

                if(cartModel == null)
                {
                    // No records exists in database for that product for that user
                    await _unitOfWork.ShoppingCart.Add(item);
                }
                else
                {
                    cartModel.Count += item.Count;
                    //_unitOfWork.ShoppingCart.Update(cartModel);
                }

                await _unitOfWork.SaveAsync();

                // Configure Sessions for Cart
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == item.ApplicationUserId).ToList().Count();

                //HttpContext.Session.SetObject(SD.SessionShoppingCart, item);
                HttpContext.Session.SetInt32(SD.SessionShoppingCart, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                // includeProperties: "Category" automatically include a Category in a Product
                var model = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == item.ProductId, includeProperties: "Category");

                ShoppingCart cartModel = new ShoppingCart()
                {
                    Product =  model,
                    ProductId = model.Id
                };

                return View(cartModel);
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult EmptyPage()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}