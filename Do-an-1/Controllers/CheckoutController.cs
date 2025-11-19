using System;
using System.Collections.Generic;
using System.Linq;
using Do_an_1.Models;
using Do_an_1.Models.ViewModels;
using Do_an_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly FashionStoreDbContext _context;
        private readonly IVnPayService _vnPayService;
        private const string CartSessionKey = "Cart";
        private const string CheckoutSessionKey = "CheckoutInfo";

        public CheckoutController(FashionStoreDbContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["CheckoutMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutSession = HttpContext.Session.GetObjectFromJson<CheckoutSessionData>(CheckoutSessionKey);
            var model = new CheckoutViewModel
            {
                Items = cart,
                Form = checkoutSession?.Form ?? new CheckoutFormModel(),
                TotalAmount = cart.Sum(item => (item.Price ?? 0) * item.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePayment(CheckoutFormModel form)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["CheckoutMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = new CheckoutViewModel
                {
                    Items = cart,
                    Form = form,
                    TotalAmount = cart.Sum(item => (item.Price ?? 0) * item.Quantity)
                };
                return View("Index", invalidModel);
            }

            var orderCode = DateTime.UtcNow.Ticks.ToString();
            var totalAmount = cart.Sum(item => (item.Price ?? 0) * item.Quantity);
            var request = new VnPayRequest
            {
                OrderId = orderCode,
                Amount = totalAmount,
                OrderDescription = $"Thanh toan don hang {orderCode}"
            };

            var checkoutSession = new CheckoutSessionData
            {
                TransactionRef = orderCode,
                TotalAmount = totalAmount,
                Form = form
            };

            HttpContext.Session.SetObjectAsJson(CheckoutSessionKey, checkoutSession);

            var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, request);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var query = Request.Query;
            if (!_vnPayService.ValidateSignature(query))
            {
                return View("Result", new CheckoutResultViewModel
                {
                    IsSuccess = false,
                    Message = "Không thể xác thực phản hồi từ VNPAY. Vui lòng thử lại."
                });
            }

            var responseCode = query["vnp_ResponseCode"].ToString();
            var txnRef = query["vnp_TxnRef"].ToString();

            var checkoutSession = HttpContext.Session.GetObjectFromJson<CheckoutSessionData>(CheckoutSessionKey);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (checkoutSession == null || cart.Count == 0 || checkoutSession.TransactionRef != txnRef)
            {
                return View("Result", new CheckoutResultViewModel
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy thông tin đơn hàng. Vui lòng liên hệ hỗ trợ."
                });
            }

            if (!string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase))
            {
                return View("Result", new CheckoutResultViewModel
                {
                    IsSuccess = false,
                    Message = "Thanh toán không thành công. Vui lòng thử lại.",
                    OrderCode = checkoutSession.TransactionRef,
                    TotalAmount = checkoutSession.TotalAmount
                });
            }

            var order = new TbOrder
            {
                Code = checkoutSession.TransactionRef,
                ShippingAddress = $"{checkoutSession.Form.FullName} - {checkoutSession.Form.PhoneNumber} - {checkoutSession.Form.Address}",
                TotalAmount = checkoutSession.TotalAmount,
                CreatedDate = DateTime.Now
            };

            _context.TbOrders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var detail = new TbOrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Price = item.Price ?? 0,
                    Quantity = item.Quantity
                };
                _context.TbOrderDetails.Add(detail);
            }

            _context.SaveChanges();

            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CheckoutSessionKey);

            return View("Result", new CheckoutResultViewModel
            {
                IsSuccess = true,
                Message = "Thanh toán thành công. Cảm ơn bạn đã mua sắm!",
                OrderCode = order.Code,
                TotalAmount = order.TotalAmount ?? 0
            });
        }
    }
}
