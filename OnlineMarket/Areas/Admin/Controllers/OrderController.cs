using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineMarket.DataAccess.Repository.IRepository;
using OnlineMarket.Models;
using OnlineMarket.Models.ViewModels;
using OnlineMarket.Utility;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderDetailsViewModel OrderDetailsViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Details(int id)
        {
            OrderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(i => i.OrderId == id, includeProperties: "Product")
            };

            return View(OrderDetailsViewModel);
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == id);

            orderHeader.OrderStatus = SD.StatusInProcess;
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == OrderDetailsViewModel.OrderHeader.Id);

            orderHeader.TrackingNumber = OrderDetailsViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            await _unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == id);

            if(orderHeader.PaymentStatus == SD.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotalSum * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };

                var service = new RefundService();
                Refund refund = await service.CreateAsync(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            await _unitOfWork.SaveAsync();

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser"); //User?
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser"); //User?
            }

            switch (status)
            {
                case "pending": orderHeaderList = orderHeaderList.Where(i => i.PaymentStatus == SD.PaymentStatusDelayedPayment); break;

                case "inprocess": orderHeaderList = orderHeaderList.Where(i => 
                       i.OrderStatus == SD.StatusApproved 
                    || i.OrderStatus ==SD.StatusInProcess 
                    || i.OrderStatus == SD.StatusPending); break;

                case "completed": orderHeaderList = orderHeaderList.Where(i => i.OrderStatus == SD.StatusShipped); break;

                case "rejected": orderHeaderList = orderHeaderList.Where(i => 
                       i.OrderStatus == SD.StatusCancelled
                    || i.OrderStatus == SD.StatusRefunded 
                    || i.OrderStatus == SD.PaymentStatusRejected); break;

                default: ; break;
            }

            return Json(new { data = orderHeaderList });
        }

        #endregion
    }
}
