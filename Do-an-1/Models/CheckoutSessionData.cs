using Do_an_1.Models.ViewModels;

namespace Do_an_1.Models
{
    public class CheckoutSessionData
    {
        public string TransactionRef { get; set; } = string.Empty;
        public CheckoutFormModel Form { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}

