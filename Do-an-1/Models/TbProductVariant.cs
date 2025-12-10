using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbProductVariant
{
    public int VariantId { get; set; }

    public int? ProductId { get; set; }

    public int? ColorId { get; set; }

    public int? SizeId { get; set; }

    public string? Image { get; set; }

    public string? Sku { get; set; }

    public decimal? Price { get; set; }

    public decimal? PriceSale { get; set; }

    public int? Quantity { get; set; }

    public bool? IsActive { get; set; }

    public virtual TbColor? Color { get; set; }

    public virtual TbProduct? Product { get; set; }

    public virtual TbSize? Size { get; set; }
}
