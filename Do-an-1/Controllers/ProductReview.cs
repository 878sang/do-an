using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class ProductReview : Controller
    {
        private readonly FashionStoreDbContext _context;

        public ProductReview(FashionStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(int productId, int rating, string name, string phone, string email, string detail)
        {
            //var userId = HttpContext.Session.GetInt32("UserId");
            try
            {

                TbProductReview review = new TbProductReview();

                review.ProductId = productId;
                review.Name = name;
                review.Star = rating;
                review.Email = email;
                review.CreatedDate = DateTime.Now;
                review.Detail = detail;
                review.IsActive = true;
                _context.Add(review);
                _context.SaveChanges();

                var product = _context.TbProducts.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    // Lấy tất cả review active
                    var reviews = _context.TbProductReviews
                        .Where(r => r.ProductId == productId && r.IsActive == true)
                        .ToList();

                    double avgStar = reviews.Any()
                ? reviews.Average(r => (double?)r.Star) ?? 0
                : 0;

                    product.Star = avgStar;

                    _context.Update(product);
                    _context.SaveChanges();
                }
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { status = false });
            }
        }
    }
}
