using Do_an_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Do_an_1.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly FashionStoreDbContext _context;
        public MenuTopViewComponent(FashionStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = _context.TbMenus.Where(m => (bool)m.IsActive).
            OrderBy(m => m.Position).ToList();
            return await Task.FromResult<IViewComponentResult>(View(items));
        }

    }
}
