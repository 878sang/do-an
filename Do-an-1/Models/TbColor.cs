using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbColor
{
    public int ColorId { get; set; }

    public string? ColorName { get; set; }

    public string? ColorCode { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbProductVariant> TbProductVariants { get; set; } = new List<TbProductVariant>();
}
