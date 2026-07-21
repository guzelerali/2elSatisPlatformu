namespace ReStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string SellerEmail { get; set; }
        public string ImagePath { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }
        public string Brand { get; set; }
        public string Size { get; set; }
    }
}
