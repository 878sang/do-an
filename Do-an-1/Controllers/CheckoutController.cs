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
        private const string BuyNowSessionKey = "BuyNowItem";

        public CheckoutController(FashionStoreDbContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var buyNowItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var useBuyNow = buyNowItems.Any();
            var items = useBuyNow ? buyNowItems : cart;

            if (!items.Any())
            {
                TempData["CheckoutMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutSession = HttpContext.Session.GetObjectFromJson<CheckoutSessionData>(CheckoutSessionKey);
            var model = new CheckoutViewModel
            {
                Items = items,
                Form = checkoutSession?.Form ?? new CheckoutFormModel(),
                TotalAmount = items.Sum(item => (item.Price ?? 0) * item.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePayment(CheckoutFormModel form)
        {
            var buyNowItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var useBuyNow = buyNowItems.Any();
            var items = useBuyNow ? buyNowItems : cart;

            if (!items.Any())
            {
                TempData["CheckoutMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = new CheckoutViewModel
                {
                    Items = items,
                    Form = form,
                    TotalAmount = items.Sum(item => (item.Price ?? 0) * item.Quantity)
                };
                return View("Index", invalidModel);
            }

            var orderCode = DateTime.UtcNow.Ticks.ToString();
            var totalAmount = items.Sum(item => (item.Price ?? 0) * item.Quantity);
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
            var buyNowItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var useBuyNow = buyNowItems.Any();
            var items = useBuyNow ? buyNowItems : cart;

            if (checkoutSession == null || items.Count == 0 || checkoutSession.TransactionRef != txnRef)
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

            foreach (var item in items)
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
            HttpContext.Session.Remove(BuyNowSessionKey);

            return View("Result", new CheckoutResultViewModel
            {
                IsSuccess = true,
                Message = "Thanh toán thành công. Cảm ơn bạn đã mua sắm!",
                OrderCode = order.Code,
                TotalAmount = order.TotalAmount ?? 0
            });
        }

        [HttpPost]
        public IActionResult BuyNow([FromBody] BuyNowRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var product = _context.TbProducts.FirstOrDefault(p => p.ProductId == request.ProductId);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            var item = new CartItem
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Image = product.Image,
                Price = product.PriceSale ?? product.Price,
                Quantity = request.Quantity,
                Size = request.Size,
                Color = request.Color
            };

            HttpContext.Session.SetObjectAsJson(BuyNowSessionKey, new List<CartItem> { item });
            TempData["CheckoutMessage"] = null;
            return Json(new { success = true, redirect = Url.Action("Index", "Checkout") });
        }
    }
}
