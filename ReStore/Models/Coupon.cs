using System;

namespace ReStore.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal? Percent { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Active { get; set; }
    }
}
