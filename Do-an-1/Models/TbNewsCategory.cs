using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbNewsCategory
{
    public int NewsCategoryId { get; set; }

    public string? Title { get; set; }

    public string? Alias { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<TbNews> TbNews { get; set; } = new List<TbNews>();
}
