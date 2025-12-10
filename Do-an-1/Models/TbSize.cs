using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbSize
{
    public int SizeId { get; set; }

    public string? SizeName { get; set; }

    public int? SizeOrder { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbProductVariant> TbProductVariants { get; set; } = new List<TbProductVariant>();
}
