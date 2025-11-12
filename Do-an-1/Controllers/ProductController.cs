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
            return View();
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
            .Where(v => v.ProductId == id && v.IsActive == true)
            .Select(v => v.Size.SizeName)
            .Distinct()
            .ToList();
            ViewBag.productColor = _context.TbProductVariants
            .Include(v => v.Color)
            .Where(v => v.ProductId == id && v.IsActive == true)
            .Select(v => v.Color)
            .Distinct()
            .ToList();
            return View(product);
        }
    }
}
