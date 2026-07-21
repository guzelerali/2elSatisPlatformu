namespace ReStore.Models
{
    public class OrderItem
    {
        public int ID { get; set; }
        public int SiparisID { get; set; }
        public int ProductId { get; set; }
        public string UrunAdi { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
