using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class BlogController : Controller
    {
        private readonly FashionStoreDbContext _context;
        public BlogController(FashionStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var blogs = _context.TbBlogs
        .Where(b => b.IsActive == true)
        .OrderByDescending(b => b.CreatedDate)
        .ToList();
            return View(blogs);
        }
    }
}
