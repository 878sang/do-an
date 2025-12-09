using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Do_an_1.Models;
using Do_an_1.Models.ViewModels;

namespace Do_an_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly FashionStoreDbContext _context;

        public ProductsController(FashionStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var fashionStoreDbContext = _context.TbProducts
                .Include(t => t.CategoryProduct)
                .Include(t => t.TbProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(t => t.TbProductVariants)
                    .ThenInclude(v => v.Color)
                .ToListAsync();

            return View(await fashionStoreDbContext);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProduct = await _context.TbProducts
                .Include(t => t.CategoryProduct)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (tbProduct == null)
            {
                return NotFound();
            }

            return View(tbProduct);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryProductId"] = new SelectList(_context.TbProductCategories, "CategoryProductId", "Title");
            ViewData["Colors"] = new SelectList(_context.TbColors.Where(c => c.IsActive == true), "ColorId", "ColorName");
            ViewData["Sizes"] = new SelectList(_context.TbSizes.Where(s => s.IsActive == true), "SizeId", "SizeName");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new TbProduct
                {
                    Title = model.Title,
                    Alias = model.Alias,
                    CategoryProductId = model.CategoryProductId,
                    Description = model.Description,
                    Detail = model.Detail,
                    Image = model.Image,
                    Price = model.Price,
                    PriceSale = model.PriceSale,
                    Quantity = model.Quantity,
                    IsNew = model.IsNew,
                    IsBestSeller = model.IsBestSeller,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.Now,
                };
                _context.Add(product);
                await _context.SaveChangesAsync();
                // Lưu các biến thể
                if (model.Variants != null && model.Variants.Count > 0)
                {
                    foreach (var v in model.Variants)
                    {
                        var variant = new TbProductVariant
                        {
                            ProductId = product.ProductId,
                            ColorId = v.ColorId,
                            SizeId = v.SizeId,
                            Sku = v.Sku,
                            Price = v.Price,
                            PriceSale = v.PriceSale,
                            Quantity = v.Quantity,
                            IsActive = v.IsActive
                        };
                        _context.TbProductVariants.Add(variant);
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryProductId"] = new SelectList(_context.TbProductCategories, "CategoryProductId", "CategoryProductId", model.CategoryProductId);
            ViewData["Colors"] = new SelectList(_context.TbColors.Where(c => c.IsActive == true), "ColorId", "ColorName");
            ViewData["Sizes"] = new SelectList(_context.TbSizes.Where(s => s.IsActive == true), "SizeId", "SizeName");
            return View(model);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProduct = await _context.TbProducts
                .Include(p => p.TbProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.TbProductVariants)
                    .ThenInclude(v => v.Size)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (tbProduct == null)
            {
                return NotFound();
            }
            ViewData["CategoryProductId"] = new SelectList(_context.TbProductCategories, "CategoryProductId", "CategoryProductId", tbProduct.CategoryProductId);
            ViewData["Colors"] = new SelectList(_context.TbColors.Where(c => c.IsActive == true), "ColorId", "ColorName");
            ViewData["Sizes"] = new SelectList(_context.TbSizes.Where(s => s.IsActive == true), "SizeId", "SizeName");
            return View(tbProduct);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Alias,CategoryProductId,Description,Detail,Image,Price,PriceSale,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,IsNew,IsBestSeller,IsActive,Quantity,Star")] TbProduct tbProduct, IFormCollection form)
        {
            if (id != tbProduct.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin sản phẩm
                    _context.Update(tbProduct);

                    // Xử lý các biến thể
                    var deletedVariantIds = new List<int>();
                    var deletedVariants = form["DeletedVariants"].ToList();
                    foreach (var deletedId in deletedVariants)
                    {
                        if (int.TryParse(deletedId, out int deletedVariantId))
                        {
                            deletedVariantIds.Add(deletedVariantId);
                        }
                    }

                    // Xóa các biến thể đã được đánh dấu xóa
                    if (deletedVariantIds.Any())
                    {
                        var variantsToDelete = await _context.TbProductVariants
                            .Where(v => deletedVariantIds.Contains(v.VariantId))
                            .ToListAsync();
                        _context.TbProductVariants.RemoveRange(variantsToDelete);
                    }

                    // Lấy danh sách biến thể từ form
                    var variantKeys = form.Keys.Where(k => k.StartsWith("Variants[") && k.Contains("].VariantId")).ToList();

                    foreach (var key in variantKeys)
                    {
                        // Lấy index từ key: "Variants[0].VariantId" -> 0
                        var match = Regex.Match(key, @"Variants\[(\d+)\]");
                        if (!match.Success) continue;

                        var index = match.Groups[1].Value;
                        var variantIdStr = form[$"Variants[{index}].VariantId"].ToString();
                        int variantId = 0;
                        int.TryParse(variantIdStr, out variantId);

                        var colorIdStr = form[$"Variants[{index}].ColorId"].ToString();
                        var sizeIdStr = form[$"Variants[{index}].SizeId"].ToString();

                        // Bỏ qua nếu không có ColorId hoặc SizeId
                        if (string.IsNullOrEmpty(colorIdStr) || string.IsNullOrEmpty(sizeIdStr))
                            continue;

                        int colorId = int.Parse(colorIdStr);
                        int sizeId = int.Parse(sizeIdStr);
                        var sku = form[$"Variants[{index}].Sku"].ToString();
                        var priceStr = form[$"Variants[{index}].Price"].ToString();
                        var priceSaleStr = form[$"Variants[{index}].PriceSale"].ToString();
                        var quantityStr = form[$"Variants[{index}].Quantity"].ToString();
                        var isActive = form[$"Variants[{index}].IsActive"].ToString() == "true";

                        if (variantId > 0)
                        {
                            // Cập nhật biến thể hiện có
                            var existingVariant = await _context.TbProductVariants
                                .FirstOrDefaultAsync(v => v.VariantId == variantId);

                            if (existingVariant != null && !deletedVariantIds.Contains(variantId))
                            {
                                existingVariant.ColorId = colorId;
                                existingVariant.SizeId = sizeId;
                                existingVariant.Sku = sku;
                                existingVariant.Price = !string.IsNullOrEmpty(priceStr) ? decimal.Parse(priceStr) : null;
                                existingVariant.PriceSale = !string.IsNullOrEmpty(priceSaleStr) ? decimal.Parse(priceSaleStr) : null;
                                existingVariant.Quantity = !string.IsNullOrEmpty(quantityStr) ? int.Parse(quantityStr) : null;
                                existingVariant.IsActive = isActive;
                                _context.Update(existingVariant);
                            }
                        }
                        else
                        {
                            // Thêm biến thể mới
                            var newVariant = new TbProductVariant
                            {
                                ProductId = tbProduct.ProductId,
                                ColorId = colorId,
                                SizeId = sizeId,
                                Sku = sku,
                                Price = !string.IsNullOrEmpty(priceStr) ? decimal.Parse(priceStr) : null,
                                PriceSale = !string.IsNullOrEmpty(priceSaleStr) ? decimal.Parse(priceSaleStr) : null,
                                Quantity = !string.IsNullOrEmpty(quantityStr) ? int.Parse(quantityStr) : null,
                                IsActive = isActive
                            };
                            _context.TbProductVariants.Add(newVariant);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbProductExists(tbProduct.ProductId))
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

            // Reload dữ liệu nếu có lỗi
            var tbProductWithVariants = await _context.TbProducts
                .Include(p => p.TbProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.TbProductVariants)
                    .ThenInclude(v => v.Size)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            ViewData["CategoryProductId"] = new SelectList(_context.TbProductCategories, "CategoryProductId", "CategoryProductId", tbProduct.CategoryProductId);
            ViewData["Colors"] = new SelectList(_context.TbColors.Where(c => c.IsActive == true), "ColorId", "ColorName");
            ViewData["Sizes"] = new SelectList(_context.TbSizes.Where(s => s.IsActive == true), "SizeId", "SizeName");
            return View(tbProductWithVariants ?? tbProduct);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProduct = await _context.TbProducts
                .Include(t => t.CategoryProduct)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (tbProduct == null)
            {
                return NotFound();
            }

            return View(tbProduct);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tbProduct = await _context.TbProducts.FindAsync(id);
            if (tbProduct != null)
            {
                _context.TbProducts.Remove(tbProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbProductExists(int id)
        {
            return _context.TbProducts.Any(e => e.ProductId == id);
        }
    }
}
