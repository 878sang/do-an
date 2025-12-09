using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public OrdersController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string status = "all")
        {
            var query = _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.TbOrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            // Lọc theo trạng thái
            if (status != "all")
            {
                var statusId = GetStatusId(status);
                if (statusId.HasValue)
                {
                    query = query.Where(o => o.OrderStatusId == statusId.Value);
                }
            }

            ViewBag.Status = status;
            ViewBag.StatusList = new SelectList(new[]
            {
                new { Value = "all", Text = "Tất cả đơn hàng" },
                new { Value = "new", Text = "Đơn hàng mới" },
                new { Value = "processing", Text = "Đang xử lý" },
                new { Value = "shipping", Text = "Đang giao hàng" },
                new { Value = "completed", Text = "Đã hoàn thành" },
                new { Value = "cancelled", Text = "Đã hủy" },
                new { Value = "returned", Text = "Hoàn trả" }
            }, "Value", "Text", status);

            return View(await query.OrderByDescending(o => o.CreatedDate).ToListAsync());
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.TbOrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderStatuses = new SelectList(_context.TbOrderStatuses, "OrderStatusId", "Name", order.OrderStatusId);
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int orderStatusId)
        {
            var order = await _context.TbOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatusId = orderStatusId;
            order.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.TbOrders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.TbOrders
                .Include(o => o.TbOrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                _context.TbOrderDetails.RemoveRange(order.TbOrderDetails);
                _context.TbOrders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private int? GetStatusId(string status)
        {
            return status switch
            {
                "new" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Mới"))?.OrderStatusId,
                "processing" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Xử lý"))?.OrderStatusId,
                "shipping" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Giao hàng"))?.OrderStatusId,
                "completed" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Hoàn thành"))?.OrderStatusId,
                "cancelled" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Hủy"))?.OrderStatusId,
                "returned" => _context.TbOrderStatuses.FirstOrDefault(s => s.Name.Contains("Hoàn trả"))?.OrderStatusId,
                _ => null
            };
        }
    }
}

