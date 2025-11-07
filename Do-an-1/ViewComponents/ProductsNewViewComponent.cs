using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.ViewComponents
{
    public class ProductsNewViewComponent : ViewComponent
    {
        private readonly FashionStoreDbContext _context;
        public ProductsNewViewComponent(FashionStoreDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = _context.TbProducts.Include(m => m.CategoryProduct)
    .Where(m => (bool)m.IsActive).Where(m => m.IsNew == true);
            return await Task.FromResult<IViewComponentResult>
            (View(items.OrderByDescending(m => m.ProductId).ToList()));
        }
    }
}
