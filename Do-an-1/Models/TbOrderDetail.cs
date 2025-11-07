using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbOrderDetail
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public virtual TbOrder Order { get; set; } = null!;

    public virtual TbProduct Product { get; set; } = null!;
}
