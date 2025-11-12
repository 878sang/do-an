using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
