using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InterfaceController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public InterfaceController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Interface/Homepage
        public IActionResult Homepage()
        {
            return View();
        }

        // GET: Admin/Interface/Menu
        public async Task<IActionResult> Menu()
        {
            var menus = await _context.TbMenus.ToListAsync();
            return View(menus);
        }

        // GET: Admin/Interface/Slider
        public IActionResult Slider()
        {
            return View();
        }

        // GET: Admin/Interface/ContentPages
        public IActionResult ContentPages()
        {
            return View();
        }
    }
}

