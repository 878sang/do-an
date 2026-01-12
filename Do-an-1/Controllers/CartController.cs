using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.Controllers
{
    public class CartController : Controller
    {
        private readonly FashionStoreDbContext _context;
        public CartController(FashionStoreDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartRequest req)
        {
            var product = _context.TbProducts.FirstOrDefault(p => p.ProductId == req.Id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Tìm sản phẩm trùng (cùng ProductId, Size và Color)
            var existing = cart.FirstOrDefault(x =>
                x.ProductId == req.Id &&
                x.Size == req.Size &&
                x.Color == req.Color);

            if (existing != null)
            {
                // Cộng dồn số lượng
                existing.Quantity += req.Quantity;
            }
            else
            {
                // Thêm sản phẩm mới
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Title = product.Title,
                    Image = product.Image,
                    Price = product.PriceSale ?? product.Price,
                    Quantity = req.Quantity,
                    Size = req.Size,
                    Color = req.Color,
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true, cartCount = cart.Sum(x => x.Quantity) });
        }

        [HttpGet]
        public IActionResult MiniCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return PartialView("_MiniCartPartial", cart);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
               ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest req)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.ProductId == req.Id);
            if (item == null) return Json(new { success = false, message = "Item not found" });

            item.Quantity = req.Quantity > 0 ? req.Quantity : 1;
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            var total = cart.Sum(x => x.Price.GetValueOrDefault() * x.Quantity);
            return Json(new { success = true, cartCount = cart.Sum(x => x.Quantity), total });
        }

        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] CartRequest req)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.ProductId == req.Id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            var total = cart.Sum(x => x.Price.GetValueOrDefault() * x.Quantity);
            return Json(new { success = true, cartCount = cart.Sum(x => x.Quantity), total });
        }

        public class UpdateQuantityRequest
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
        }

        public class CartRequest
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public string Image { get; set; }
            public decimal? Price { get; set; }
            public string Size { get; set; }
            public string Color { get; set; }
        }
    }
}
