using System.Collections.Generic;
using Do_an_1.Models;

namespace Do_an_1.Models.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public TbCustomer Customer { get; set; }
        public IEnumerable<TbOrder> Orders { get; set; }
    }
}
