using System;
using System.Collections.Generic;

namespace Do_an_1.Models;

public partial class TbProductReview
{
    public int ProductReviewId { get; set; }

    public int? CustomerId { get; set; }

    public string? Detail { get; set; }

    public int? Star { get; set; }

    public int? ProductId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual TbCustomer? Customer { get; set; }

    public virtual TbProduct? Product { get; set; }
}
