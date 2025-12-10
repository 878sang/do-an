using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SupportController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public SupportController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Support
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.TbContacts
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
            return View(contacts);
        }
    }
}

