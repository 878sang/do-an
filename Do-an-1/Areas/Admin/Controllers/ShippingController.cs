using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShippingController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public ShippingController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Shipping/Methods
        public IActionResult Methods()
        {
            return View();
        }

        // GET: Admin/Shipping/Carriers
        public IActionResult Carriers()
        {
            return View();
        }

        // GET: Admin/Shipping/Fees
        public IActionResult Fees()
        {
            return View();
        }

        // GET: Admin/Shipping/Tracking
        public async Task<IActionResult> Tracking()
        {
            var orders = await _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .ToListAsync();
            
            // Filter after loading to avoid expression tree issues with null propagating operators
            orders = orders.Where(o => o.OrderStatus != null && o.OrderStatus.Name != null && 
                (o.OrderStatus.Name.Contains("Giao hàng") || o.OrderStatus.Name.Contains("Xử lý")))
                .ToList();
            
            return View(orders);
        }
    }
}

