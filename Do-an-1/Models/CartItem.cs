namespace Do_an_1.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string Weight { get; set; } // Thêm thuộc tính weight cho size/khối lượng
    }
}
