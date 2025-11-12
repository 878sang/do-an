using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.Controllers
{
    public class ShopController : Controller
    {
        private readonly FashionStoreDbContext _context;
        public ShopController(FashionStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var product = _context.TbProducts
        .Where(b => b.IsActive == true)
        .OrderByDescending(b => b.CreatedDate)
        .ToList();
            return View(product);
        }
    }
}
