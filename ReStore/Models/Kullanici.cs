using System;
using System.Collections.Generic;
using System.Text;

namespace ReStore.Models
{
    public class Kullanici
    {
        public string KullaniciAd { get; set; }
        public string KullaniciSifre { get; set; }
        public string KullaniciEmail { get; set; }
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        public string? Telefon { get; set; }
        public string? ProfilResmi { get; set; }
    }
}
