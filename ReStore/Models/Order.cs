using System;
using System.Collections.Generic;

namespace ReStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int SiparisID { get; set; }
        public int KullaniciID { get; set; }
        public int AdresID { get; set; }
        public string AdresBaslik { get; set; } = string.Empty;
        public string UserEmail { get; set; }
        public decimal Total { get; set; }
        public decimal ToplamFiyat { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Tarih { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
