using MySql.Data.MySqlClient;
using ReStore.Config;
using ReStore.Models;

namespace ReStore.Dao
{
    public class AddressDao
    {
        public AddressDao()
        {
            EnsureTable();
        }

        private void EnsureTable()
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            const string sql = @"
CREATE TABLE IF NOT EXISTS Adresler (
    AdresID INT AUTO_INCREMENT PRIMARY KEY,
    KullaniciID INT NOT NULL,
    Baslik VARCHAR(100) NOT NULL,
    AdresDetay VARCHAR(500) NOT NULL,
    Sehir VARCHAR(100) NOT NULL,
    Ilce VARCHAR(100) NOT NULL,
    Varsayilan TINYINT(1) NOT NULL DEFAULT 0
);";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public List<Adres> GetByKullaniciId(int kullaniciId)
        {
            var result = new List<Adres>();
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            const string sql = @"SELECT AdresID, KullaniciID, Baslik, AdresDetay, Sehir, Ilce, Varsayilan
                                 FROM Adresler WHERE KullaniciID=@kullaniciId
                                 ORDER BY Varsayilan DESC, AdresID DESC";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kullaniciId", kullaniciId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Adres
                {
                    AdresID = reader.GetInt32("AdresID"),
                    KullaniciID = reader.GetInt32("KullaniciID"),
                    Baslik = reader.GetString("Baslik"),
                    AdresDetay = reader.GetString("AdresDetay"),
                    Sehir = reader.GetString("Sehir"),
                    Ilce = reader.GetString("Ilce"),
                    Varsayilan = reader.GetBoolean("Varsayilan")
                });
            }

            return result;
        }

        public int Add(Adres adres)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                if (adres.Varsayilan)
                {
                    ResetDefaultAddress(conn, tran, adres.KullaniciID);
                }

                const string sql = @"INSERT INTO Adresler (KullaniciID, Baslik, AdresDetay, Sehir, Ilce, Varsayilan)
                                     VALUES (@kullaniciId, @baslik, @adresDetay, @sehir, @ilce, @varsayilan)";
                using var cmd = new MySqlCommand(sql, conn, tran);
                cmd.Parameters.AddWithValue("@kullaniciId", adres.KullaniciID);
                cmd.Parameters.AddWithValue("@baslik", adres.Baslik);
                cmd.Parameters.AddWithValue("@adresDetay", adres.AdresDetay);
                cmd.Parameters.AddWithValue("@sehir", adres.Sehir);
                cmd.Parameters.AddWithValue("@ilce", adres.Ilce);
                cmd.Parameters.AddWithValue("@varsayilan", adres.Varsayilan);
                cmd.ExecuteNonQuery();

                var newId = (int)cmd.LastInsertedId;
                tran.Commit();
                return newId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public bool Update(Adres adres)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                if (adres.Varsayilan)
                {
                    ResetDefaultAddress(conn, tran, adres.KullaniciID);
                }

                const string sql = @"UPDATE Adresler
                                     SET Baslik=@baslik, AdresDetay=@adresDetay, Sehir=@sehir, Ilce=@ilce, Varsayilan=@varsayilan
                                     WHERE AdresID=@adresId AND KullaniciID=@kullaniciId";
                using var cmd = new MySqlCommand(sql, conn, tran);
                cmd.Parameters.AddWithValue("@adresId", adres.AdresID);
                cmd.Parameters.AddWithValue("@kullaniciId", adres.KullaniciID);
                cmd.Parameters.AddWithValue("@baslik", adres.Baslik);
                cmd.Parameters.AddWithValue("@adresDetay", adres.AdresDetay);
                cmd.Parameters.AddWithValue("@sehir", adres.Sehir);
                cmd.Parameters.AddWithValue("@ilce", adres.Ilce);
                cmd.Parameters.AddWithValue("@varsayilan", adres.Varsayilan);
                var affected = cmd.ExecuteNonQuery();
                tran.Commit();
                return affected > 0;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public bool Delete(int adresId, int kullaniciId)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            const string sql = "DELETE FROM Adresler WHERE AdresID=@adresId AND KullaniciID=@kullaniciId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@adresId", adresId);
            cmd.Parameters.AddWithValue("@kullaniciId", kullaniciId);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void ResetDefaultAddress(MySqlConnection conn, MySqlTransaction tran, int kullaniciId)
        {
            const string sql = "UPDATE Adresler SET Varsayilan=0 WHERE KullaniciID=@kullaniciId";
            using var cmd = new MySqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@kullaniciId", kullaniciId);
            cmd.ExecuteNonQuery();
        }
    }
}
