using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Do_an_1.ViewComponents
{
    public class ProductsBestSellerViewComponent : ViewComponent
    {
        private readonly FashionStoreDbContext _context;
        public ProductsBestSellerViewComponent(FashionStoreDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = _context.TbProducts.Include(m => m.CategoryProduct)
    .Where(m => (bool)m.IsActive).Where(m => m.IsBestSeller == true);
            return await Task.FromResult<IViewComponentResult>
            (View(items.OrderByDescending(m => m.ProductId).ToList()));
        }
    }
}
