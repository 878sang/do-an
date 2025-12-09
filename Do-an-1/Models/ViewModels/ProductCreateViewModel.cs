using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Do_an_1.Models.ViewModels
{
    public class ProductVariantCreateModel
    {
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public string? Sku { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceSale { get; set; }
        public int? Quantity { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductCreateViewModel
    {
        // Các trường sản phẩm cơ bản
        public string? Title { get; set; }
        public string? Alias { get; set; }
        public int? CategoryProductId { get; set; }
        public string? Description { get; set; }
        public string? Detail { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceSale { get; set; }
        public int? Quantity { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsActive { get; set; }
        public int? Star { get; set; }

        // Thông tin biến thể
        public List<ProductVariantCreateModel> Variants { get; set; } = new();
    }
}
