using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using System.Collections.Generic;

namespace Do_an_1.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }
    }
}
