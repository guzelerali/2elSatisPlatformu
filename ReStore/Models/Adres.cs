namespace ReStore.Models
{
    public class Adres
    {
        public int AdresID { get; set; }
        public int KullaniciID { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string AdresDetay { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public bool Varsayilan { get; set; }
    }
}
