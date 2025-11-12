using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public BlogsController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Blogs
        public async Task<IActionResult> Index()
        {
            var fashionStoreDbContext = _context.TbBlogs.Include(t => t.Account).Include(t => t.BlogCategory);
            return View(await fashionStoreDbContext.ToListAsync());
        }

        // GET: Admin/Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbBlog = await _context.TbBlogs
                .Include(t => t.Account)
                .Include(t => t.BlogCategory)
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (tbBlog == null)
            {
                return NotFound();
            }

            return View(tbBlog);
        }

        // GET: Admin/Blogs/Create
        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.TbAccounts, "AccountId", "AccountId");
            ViewData["BlogCategoryId"] = new SelectList(_context.TbBlogCategories, "BlogCategoryId", "BlogCategoryId");
            return View();
        }

        // POST: Admin/Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Alias,BlogCategoryId,Description,Detail,Image,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,AccountId,IsActive")] TbBlog tbBlog)
        {
            if (ModelState.IsValid)
            {
                tbBlog.Alias = Do_an_1.Utilities.Function.TitleSlugGenerationAlias(tbBlog.Title);
                _context.Add(tbBlog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.TbAccounts, "AccountId", "AccountId", tbBlog.AccountId);
            ViewData["BlogCategoryId"] = new SelectList(_context.TbBlogCategories, "BlogCategoryId", "BlogCategoryId", tbBlog.BlogCategoryId);
            return View(tbBlog);
        }

        // GET: Admin/Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbBlog = await _context.TbBlogs.FindAsync(id);
            if (tbBlog == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.TbAccounts, "AccountId", "AccountId", tbBlog.AccountId);
            ViewData["BlogCategoryId"] = new SelectList(_context.TbBlogCategories, "BlogCategoryId", "BlogCategoryId", tbBlog.BlogCategoryId);
            return View(tbBlog);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BlogId,Title,Alias,BlogCategoryId,Description,Detail,Image,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,AccountId,IsActive")] TbBlog tbBlog)
        {
            if (id != tbBlog.BlogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tbBlog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbBlogExists(tbBlog.BlogId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.TbAccounts, "AccountId", "AccountId", tbBlog.AccountId);
            ViewData["BlogCategoryId"] = new SelectList(_context.TbBlogCategories, "BlogCategoryId", "BlogCategoryId", tbBlog.BlogCategoryId);
            return View(tbBlog);
        }

        // GET: Admin/Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbBlog = await _context.TbBlogs
                .Include(t => t.Account)
                .Include(t => t.BlogCategory)
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (tbBlog == null)
            {
                return NotFound();
            }

            return View(tbBlog);
        }

        // POST: Admin/Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tbBlog = await _context.TbBlogs.FindAsync(id);
            if (tbBlog != null)
            {
                _context.TbBlogs.Remove(tbBlog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbBlogExists(int id)
        {
            return _context.TbBlogs.Any(e => e.BlogId == id);
        }
    }
}
