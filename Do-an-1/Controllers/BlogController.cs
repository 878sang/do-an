using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            var blog = _context.TbBlogs
        .Where(b => b.IsActive == true)
        .OrderByDescending(b => b.CreatedDate)
        .ToList();
            return View(blog);
        }
        [Route("/blog/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TbBlogs == null)
            {
                return NotFound();
            }
            var blog = await _context.TbBlogs
                .Include(m => m.BlogCategory)
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewBag.BlogComment = await _context.TbBlogComments
                .Include(m=>m.Customer)
                .Where(i => i.BlogId == id)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
            ViewBag.Catetory = _context.TbBlogCategories
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var relatedBlogsQuery = _context.TbBlogs
                .Where(b => b.BlogId != blog.BlogId && b.IsActive == true);

            if (blog.BlogCategoryId.HasValue)
            {
                relatedBlogsQuery = relatedBlogsQuery
                    .Where(b => b.BlogCategoryId == blog.BlogCategoryId);
            }

            ViewBag.RelatedBlogs = await relatedBlogsQuery
                .OrderByDescending(b => b.CreatedDate)
                .Take(4)
                .ToListAsync();
            return View(blog);
        }
    }
}
