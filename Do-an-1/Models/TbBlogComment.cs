﻿using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbBlogComment
{
    public int CommentId { get; set; }

    public int? BlogId { get; set; }

    public int? CustomerId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Detail { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual TbBlog? Blog { get; set; }

    public virtual TbCustomer? Customer { get; set; }
}
