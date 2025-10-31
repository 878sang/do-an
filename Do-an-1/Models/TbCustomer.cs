using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbCustomer
{
    public int CustomerId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public DateTime? Birthday { get; set; }

    public string? Avatar { get; set; }

    public string? Location { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TbBlogComment> TbBlogComments { get; set; } = new List<TbBlogComment>();

    public virtual ICollection<TbOrder> TbOrders { get; set; } = new List<TbOrder>();

    public virtual ICollection<TbProductReview> TbProductReviews { get; set; } = new List<TbProductReview>();
}
