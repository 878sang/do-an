using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PaymentController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public PaymentController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Payment/Methods
        public IActionResult Methods()
        {
            return View();
        }

        // GET: Admin/Payment/Transactions
        public async Task<IActionResult> Transactions()
        {
            var orders = await _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: Admin/Payment/Invoices
        public async Task<IActionResult> Invoices()
        {
            var orders = await _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.TbOrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
            return View(orders);
        }
    }
}

