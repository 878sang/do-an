using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Do_an_1.Models;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.Controllers;

public class HomeController : Controller
{
    private readonly FashionStoreDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, FashionStoreDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.Blog = _context.TbBlogs
.Where(m => (bool)m.IsActive)
    .OrderByDescending(m => m.CreatedDate).ToList();
        return View();
    }

    public IActionResult Shop()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
