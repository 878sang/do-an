using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
