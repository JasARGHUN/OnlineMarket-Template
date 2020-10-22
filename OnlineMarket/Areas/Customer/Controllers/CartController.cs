using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using OnlineMarket.Models.ViewModels;
using OnlineMarket.Utility;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace OnlineMarket.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                // includeProperties: "Product" automatically include a Product in a ShoppingCart
                ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            ShoppingCartViewModel.OrderHeader.OrderTotalSum = 0;
            // includeProperties: "Company" automatically include a Company in a ApplicationUser
            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(x => x.Id == claim.Value, includeProperties: "Company");

            foreach (var i in ShoppingCartViewModel.ListCart)
            {
                i.Price = SD.GetPriceBasedOnQuantity(i.Count, i.Product.Price);

                ShoppingCartViewModel.OrderHeader.OrderTotalSum += (i.Price * i.Count);

                i.Product.Description = SD.ConvertToRawHtml(i.Product.Description);

                if(i.Product.Description.Length > 200)
                {
                    i.Product.Description = i.Product.Description.Substring(0, 100) + "...";
                }
            }

            return View(ShoppingCartViewModel);
        }

        // This action method will resend the confirmation email to user
        [HttpPost]
        [ActionName("Index")] // We use [ActionName("Index")] because 2 same action methods doesn't work
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.User.GetFirstOrDefault(x => x.Id == claim.Value);

            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code},
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var model = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId, includeProperties: "Product");

            model.Count += 1;
            model.Price = SD.GetPriceBasedOnQuantity(model.Count, model.Product.Price);

            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var model = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId, includeProperties: "Product");

            if (model.Count == 1)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == model.ApplicationUserId).ToList().Count();

                await _unitOfWork.ShoppingCart.Remove(model);
                await _unitOfWork.SaveAsync();

                HttpContext.Session.SetInt32(SD.SessionShoppingCart, count - 1);
            }
            else
            {
                model.Count -= 1;
                model.Price = SD.GetPriceBasedOnQuantity(model.Count, model.Product.Price);

                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var model = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.Id == cartId, includeProperties: "Product");

            var count = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == model.ApplicationUserId).ToList().Count();

            await _unitOfWork.ShoppingCart.Remove(model);
            await _unitOfWork.SaveAsync();

            HttpContext.Session.SetInt32(SD.SessionShoppingCart, count - 1);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new Models.OrderHeader(),
                // includeProperties: "Product" automatically include a Product in a ShoppingCart
                ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(i => i.Id == claim.Value, includeProperties: "Company");

            foreach (var i in ShoppingCartViewModel.ListCart)
            {
                i.Price = SD.GetPriceBasedOnQuantity(i.Count, i.Product.Price);

                ShoppingCartViewModel.OrderHeader.OrderTotalSum += (i.Price * i.Count);
            }

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.ApplicationUser.State;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
            ShoppingCartViewModel.OrderHeader.Address = ShoppingCartViewModel.OrderHeader.ApplicationUser.Address;
            ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartViewModel);
        }

        //Stripe test account, Login: onlineshop131020@gmail.com Password: _qwerty123456$test$

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(string stripeToken) // stripeToken contains transaction data
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.User.GetFirstOrDefault(i => i.Id == claim.Value, includeProperties: "Company");

            ShoppingCartViewModel.ListCart = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "Product");

            ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;

            await _unitOfWork.OrderHeader.Add(ShoppingCartViewModel.OrderHeader); //!
            await _unitOfWork.SaveAsync(); //!

            foreach(var i in ShoppingCartViewModel.ListCart)
            {
                i.Price = SD.GetPriceBasedOnQuantity(i.Count, i.Product.Price);

                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = i.ProductId,
                    OrderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = i.Price,
                    Count = i.Count
                };

                ShoppingCartViewModel.OrderHeader.OrderTotalSum += orderDetails.Count * orderDetails.Price;

                await _unitOfWork.OrderDetails.Add(orderDetails); //!
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartViewModel.ListCart);
            await _unitOfWork.SaveAsync();
            HttpContext.Session.SetInt32(SD.SessionShoppingCart, 0);

            if(stripeToken == null)
            {
                // Order will be created for delayed payment for authorized company
                ShoppingCartViewModel.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            else
            {
                // Payment process
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartViewModel.OrderHeader.OrderTotalSum * 100),
                    Currency = "usd",
                    Description = "Order ID: " + ShoppingCartViewModel.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = await service.CreateAsync(options); //!

                if (charge.BalanceTransactionId == null)
                {
                    ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    ShoppingCartViewModel.OrderHeader.TransactionId = charge.BalanceTransactionId;
                }

                if(charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartViewModel.OrderHeader.OrderStatus = SD.StatusApproved;
                    ShoppingCartViewModel.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            await _unitOfWork.SaveAsync();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartViewModel.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
