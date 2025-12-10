using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra nếu chưa đăng nhập thì chuyển về trang Login
            if (HttpContext.Session.GetString("AdminId") == null)
            {
                return RedirectToAction("Login", "Accounts", new { area = "Admin" });
            }
            return View();
        }
    }
}
