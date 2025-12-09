using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using System.Linq;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public ContactController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách liên hệ
        public IActionResult Index()
        {
            // Lấy danh sách sắp xếp theo ngày mới nhất
            var items = _context.TbContacts
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
            return View(items);
        }

        // 2. Xem chi tiết (đánh dấu đã đọc)
        public IActionResult Detail(int id)
        {
            var item = _context.TbContacts.Find(id);
            if (item != null)
            {
                // Cập nhật trạng thái đã đọc
                item.IsRead = true;
                _context.SaveChanges();
            }
            return View(item);
        }

        // 3. Xóa liên hệ
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _context.TbContacts.Find(id);
            if (item != null)
            {
                _context.TbContacts.Remove(item);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}