using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Xml.Linq;

namespace Do_an_1.Controllers
{
    public class BlogComment : Controller
    {
        private readonly FashionStoreDbContext _context;

        public BlogComment(FashionStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(int blogId, string detail)
        {
            var CustomerId = int.Parse(HttpContext.Session.GetString("CustomerId"));
            try
            {
                TbBlogComment comment = new TbBlogComment();

                comment.BlogId = blogId;
                comment.CustomerId = CustomerId;
                comment.CreatedDate = DateTime.Now;
                comment.Detail = detail;
                _context.Add(comment);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { status = false });
            }
        }
    }
}
