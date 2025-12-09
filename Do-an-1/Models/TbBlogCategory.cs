using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbBlogCategory
{
    public int BlogCategoryId { get; set; }

    public string? Title { get; set; }

    public string? Alias { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<TbBlog> TbBlogs { get; set; } = new List<TbBlog>();
}
