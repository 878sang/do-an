using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarketingController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public MarketingController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Marketing/Coupons
        public IActionResult Coupons()
        {
            return View();
        }

        // GET: Admin/Marketing/FlashSale
        public IActionResult FlashSale()
        {
            return View();
        }

        // GET: Admin/Marketing/Vouchers
        public IActionResult Vouchers()
        {
            return View();
        }

        // GET: Admin/Marketing/EmailMarketing
        public IActionResult EmailMarketing()
        {
            return View();
        }

        // GET: Admin/Marketing/Banners
        public IActionResult Banners()
        {
            return View();
        }
    }
}

