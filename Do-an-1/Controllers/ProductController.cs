using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.Controllers
{
    public class ProductController : Controller
    {
        private readonly FashionStoreDbContext _context;
        public ProductController(FashionStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.TbProducts
        .Where(b => b.IsActive == true)
        .OrderByDescending(b => b.CreatedDate)
        .ToList();
            return View(products);
        }
        [Route("/product/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TbProducts == null)
            {
                return NotFound();
            }
            var product = await _context.TbProducts.Include(i => i.CategoryProduct)
            .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.productSize = _context.TbProductVariants
            .Include(v => v.Size)
            .Where(v => v.ProductId == id && v.IsActive == true && v.Size != null)
            .Select(v => v.Size.SizeName)
            .Distinct()
            .ToList();
            ViewBag.productColor = _context.TbProductVariants
            .Include(v => v.Color)
            .Where(v => v.ProductId == id && v.IsActive == true)
            .Select(v => v.Color)
            .Distinct()
            .ToList();
            ViewBag.productReview = _context.TbProductReviews.
            Where(i => i.ProductId == id && (i.IsActive == true || i.IsActive == null)).OrderByDescending(r => r.CreatedDate).ToList();
            return View(product);
        }
        public IActionResult Preview(int id)
        {
            var product = _context.TbProducts.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.productSize = _context.TbProductVariants
            .Include(v => v.Size)
            .Where(v => v.ProductId == id && v.IsActive == true && v.Size != null)
            .Select(v => v.Size.SizeName)
            .Distinct()
            .ToList();
            ViewBag.productColor = _context.TbProductVariants
            .Include(v => v.Color)
            .Where(v => v.ProductId == id && v.IsActive == true)
            .Select(v => v.Color)
            .Distinct()
            .ToList();
            return PartialView("_ProductPreview", product);
        }
    }
}
