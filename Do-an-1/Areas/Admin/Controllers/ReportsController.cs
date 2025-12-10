using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public ReportsController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var orders = await _context.TbOrders
                .Where(o => o.CreatedDate >= start && o.CreatedDate <= end)
                .Include(o => o.OrderStatus)
                .ToListAsync();

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            // Filter after loading to avoid expression tree issues with null propagating operators
            var completedOrders = orders.Where(o => o.OrderStatus != null && o.OrderStatus.Name != null && o.OrderStatus.Name.Contains("Hoàn thành")).ToList();
            ViewBag.TotalRevenue = completedOrders.Sum(o => o.TotalAmount ?? 0);
            ViewBag.TotalOrders = orders.Count;
            ViewBag.CompletedOrders = completedOrders.Count;

            return View(orders);
        }

        // GET: Admin/Reports/Products
        public async Task<IActionResult> Products()
        {
            var products = await _context.TbProducts
                .Include(p => p.CategoryProduct)
                .Include(p => p.TbOrderDetails)
                .ToListAsync();

            var productReports = products.Select(p => new
            {
                Product = p,
                TotalSold = p.TbOrderDetails.Sum(od => od.Quantity),
                TotalRevenue = p.TbOrderDetails.Sum(od => od.Price * od.Quantity)
            }).OrderByDescending(x => x.TotalSold);

            ViewBag.ProductReports = productReports;
            return View(products);
        }

        // GET: Admin/Reports/Customers
        public async Task<IActionResult> Customers()
        {
            var customers = await _context.TbCustomers
                .Include(c => c.TbOrders)
                .ToListAsync();

            var customerReports = customers.Select(c => new
            {
                Customer = c,
                TotalOrders = c.TbOrders.Count,
                TotalSpent = c.TbOrders.Sum(o => o.TotalAmount ?? 0)
            }).OrderByDescending(x => x.TotalSpent);

            ViewBag.CustomerReports = customerReports;
            return View(customers);
        }

        // GET: Admin/Reports/Inventory
        public async Task<IActionResult> Inventory()
        {
            var products = await _context.TbProducts
                .Include(p => p.CategoryProduct)
                .Include(p => p.TbProductVariants)
                .ToListAsync();

            ViewBag.LowStock = products.Where(p => 
                (p.TbProductVariants.Any() ? p.TbProductVariants.Sum(v => v.Quantity) : (p.Quantity ?? 0)) < 10
            ).ToList();

            return View(products);
        }

        // GET: Admin/Reports/Profit
        public async Task<IActionResult> Profit(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var orders = await _context.TbOrders
                .Where(o => o.CreatedDate >= start && o.CreatedDate <= end)
                .Include(o => o.OrderStatus)
                .Include(o => o.TbOrderDetails)
                    .ThenInclude(od => od.Product)
                .ToListAsync();

            // Filter after loading to avoid expression tree issues with null propagating operators
            orders = orders.Where(o => o.OrderStatus != null && o.OrderStatus.Name != null && o.OrderStatus.Name.Contains("Hoàn thành")).ToList();

            var totalRevenue = orders.Sum(o => o.TotalAmount ?? 0);
            var totalCost = orders.SelectMany(o => o.TbOrderDetails)
                .Sum(od => (od.Product != null && od.Product.Price.HasValue ? od.Product.Price.Value : 0) * od.Quantity * 0.6m); // Giả sử chi phí = 60% giá bán
            var profit = totalRevenue - totalCost;

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCost = totalCost;
            ViewBag.Profit = profit;
            ViewBag.ProfitMargin = totalRevenue > 0 ? (profit / totalRevenue * 100) : 0;

            return View(orders);
        }
    }
}

