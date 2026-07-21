using ReStore.Config;
using ReStore.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ReStore.Dao
{
    public class OrderDao
    {
        public OrderDao()
        {
            EnsureTables();
        }

        private void EnsureTables()
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            string sql = @"
CREATE TABLE IF NOT EXISTS Siparisler (
  SiparisID INT AUTO_INCREMENT PRIMARY KEY,
  KullaniciID INT NOT NULL,
  AdresID INT NOT NULL,
  ToplamFiyat DECIMAL(10,2) NOT NULL,
  Tarih DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS SiparisDetay (
  ID INT AUTO_INCREMENT PRIMARY KEY,
  SiparisID INT NOT NULL,
  UrunAdi VARCHAR(255) NOT NULL,
  Fiyat DECIMAL(10,2) NOT NULL
);";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public List<Order> GetOrdersByUser(int kullaniciId)
        {
            var list = new List<Order>();
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            string sql = @"
SELECT s.SiparisID, s.KullaniciID, s.AdresID, s.ToplamFiyat, s.Tarih, IFNULL(a.Baslik, '') AS AdresBaslik
FROM Siparisler s
LEFT JOIN Adresler a ON a.AdresID = s.AdresID
WHERE s.KullaniciID=@kullaniciId
ORDER BY s.Tarih DESC";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kullaniciId", kullaniciId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var order = new Order
                {
                    Id = reader.GetInt32("SiparisID"),
                    SiparisID = reader.GetInt32("SiparisID"),
                    KullaniciID = reader.GetInt32("KullaniciID"),
                    AdresID = reader.GetInt32("AdresID"),
                    ToplamFiyat = reader.GetDecimal("ToplamFiyat"),
                    Total = reader.GetDecimal("ToplamFiyat"),
                    Tarih = reader.GetDateTime("Tarih"),
                    CreatedAt = reader.GetDateTime("Tarih"),
                    AdresBaslik = reader.GetString("AdresBaslik")
                };
                list.Add(order);
            }
            reader.Close();

            foreach (var order in list)
            {
                order.Items = GetOrderItems(order.SiparisID, conn);
            }

            return list;
        }

        public bool CreateOrder(int kullaniciId, int adresId, List<OrderItem> items)
        {
            decimal total = 0m;
            foreach (var item in items)
            {
                total += item.Fiyat > 0 ? item.Fiyat : item.UnitPrice * Math.Max(1, item.Quantity);
            }
            return CreateOrder(kullaniciId, adresId, items, total);
        }

        public bool CreateOrder(int kullaniciId, int adresId, List<OrderItem> items, decimal toplamFiyat)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                using var tran = conn.BeginTransaction();
                try
                {
                    string insOrder = "INSERT INTO Siparisler (KullaniciID, AdresID, ToplamFiyat, Tarih) VALUES (@kullaniciId, @adresId, @toplamFiyat, @tarih)";
                    using var cmd = new MySqlCommand(insOrder, conn, tran);
                    cmd.Parameters.AddWithValue("@kullaniciId", kullaniciId);
                    cmd.Parameters.AddWithValue("@adresId", adresId);
                    cmd.Parameters.AddWithValue("@toplamFiyat", toplamFiyat);
                    cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    long orderId = cmd.LastInsertedId;

                    string insItem = "INSERT INTO SiparisDetay (SiparisID, UrunAdi, Fiyat) VALUES (@siparisId, @urunAdi, @fiyat)";
                    foreach (var item in items)
                    {
                        using var cmd2 = new MySqlCommand(insItem, conn, tran);
                        cmd2.Parameters.AddWithValue("@siparisId", orderId);
                        cmd2.Parameters.AddWithValue("@urunAdi", item.UrunAdi);
                        cmd2.Parameters.AddWithValue("@fiyat", item.Fiyat > 0 ? item.Fiyat : item.UnitPrice * Math.Max(1, item.Quantity));
                        cmd2.ExecuteNonQuery();
                    }

                    tran.Commit();
                    return true;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Sipariş oluşturulurken hata: " + ex.Message, "Hata", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        private static List<OrderItem> GetOrderItems(int siparisId, MySqlConnection conn)
        {
            var list = new List<OrderItem>();
            const string sql = "SELECT ID, SiparisID, UrunAdi, Fiyat FROM SiparisDetay WHERE SiparisID=@siparisId ORDER BY ID";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@siparisId", siparisId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OrderItem
                {
                    ID = reader.GetInt32("ID"),
                    SiparisID = reader.GetInt32("SiparisID"),
                    UrunAdi = reader.GetString("UrunAdi"),
                    Fiyat = reader.GetDecimal("Fiyat"),
                    Quantity = 1,
                    UnitPrice = reader.GetDecimal("Fiyat")
                });
            }
            return list;
        }
    }
}
