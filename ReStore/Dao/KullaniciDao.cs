using ReStore.Config;
using ReStore.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace ReStore.Dao
{
    public class KullaniciDao
    {
        private bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string q = @"SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@schema AND TABLE_NAME=@table AND COLUMN_NAME=@column";
                using var cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@schema", DbConfig.DatabaseName);
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@column", columnName);
                var res = cmd.ExecuteScalar();
                if (res != null && int.TryParse(res.ToString(), out int cnt)) return cnt > 0;
            }
            catch
            {
            }
            return false;
        }

        public bool UserNameExists(string kullaniciAd)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"SELECT COUNT(*) FROM kullanici WHERE kullaniciAd=@ad";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ad", kullaniciAd);
                object result = cmd.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int count)) return count > 0;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private void EnsureTelefonColumnExists()
        {
            if (ColumnExists("kullanici", "telefon")) return;
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string alter = @"ALTER TABLE kullanici ADD COLUMN telefon VARCHAR(50) NULL";
                using var cmd = new MySqlCommand(alter, conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        private void EnsureProfilResmiColumnExists()
        {
            if (ColumnExists("kullanici", "profilResmi")) return;
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string alter = @"ALTER TABLE kullanici ADD COLUMN profilResmi VARCHAR(255) NULL";
                using var cmd = new MySqlCommand(alter, conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public Kullanici? GirisYap(string kullaniciMail, string kullaniciSifre)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                bool hasTelefon = ColumnExists("kullanici", "telefon");
                bool hasProfilResmi = ColumnExists("kullanici", "profilResmi");
                string query = hasTelefon
                    ? @"SELECT id, kullaniciAd, kullaniciSifre, kullaniciMail, COALESCE(isAdmin,0) AS isAdmin, telefon FROM kullanici WHERE kullaniciMail=@mail AND kullaniciSifre=@sifre"
                    : @"SELECT id, kullaniciAd, kullaniciSifre, kullaniciMail, COALESCE(isAdmin,0) AS isAdmin FROM kullanici WHERE kullaniciMail=@mail AND kullaniciSifre=@sifre";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                cmd.Parameters.AddWithValue("@sifre", kullaniciSifre);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int idxId = reader.GetOrdinal("id");
                    int idxAd = reader.GetOrdinal("kullaniciAd");
                    int idxSifre = reader.GetOrdinal("kullaniciSifre");
                    int idxMail = reader.GetOrdinal("kullaniciMail");

                    int idxIsAdmin = -1;
                    if (HasColumn(reader, "isAdmin")) idxIsAdmin = reader.GetOrdinal("isAdmin");

                    int idxTelefon = -1;
                    if (hasTelefon && HasColumn(reader, "telefon")) idxTelefon = reader.GetOrdinal("telefon");

                    int idxProfil = -1;
                    if (hasProfilResmi && HasColumn(reader, "profilResmi")) idxProfil = reader.GetOrdinal("profilResmi");

                    return new Kullanici
                    {
                        Id = reader.IsDBNull(idxId) ? 0 : reader.GetInt32(idxId),
                        KullaniciAd = reader.IsDBNull(idxAd) ? string.Empty : reader.GetString(idxAd),
                        KullaniciSifre = reader.IsDBNull(idxSifre) ? string.Empty : reader.GetString(idxSifre),
                        KullaniciEmail = reader.IsDBNull(idxMail) ? string.Empty : reader.GetString(idxMail),
                        IsAdmin = idxIsAdmin >= 0 && !reader.IsDBNull(idxIsAdmin) && reader.GetInt32(idxIsAdmin) == 1,
                        Telefon = idxTelefon >= 0 && !reader.IsDBNull(idxTelefon) ? reader.GetString(idxTelefon) : null,
                        ProfilResmi = idxProfil >= 0 && !reader.IsDBNull(idxProfil) ? reader.GetString(idxProfil) : null
                    };
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var log = $"{DateTime.Now:O} - GirisYap hata: {ex}\n";
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_errors.log"), log);
                }
                catch { }

                MessageBox.Show("Giriş sırasında veritabanı hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public bool KayitOl(string kullaniciAd, string kullaniciMail, string kullaniciSifre, string? telefon = null)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();

                EnsureTelefonColumnExists();
                EnsureProfilResmiColumnExists();
                bool hasTelefon = ColumnExists("kullanici", "telefon");
                bool hasProfilResmi = ColumnExists("kullanici", "profilResmi");

                string query;
                if (hasTelefon && hasProfilResmi)
                    query = @"INSERT INTO kullanici (kullaniciAd, kullaniciSifre, kullaniciMail, telefon, profilResmi) VALUES (@ad, @sifre, @mail, @telefon, @profilResmi)";
                else if (hasTelefon)
                    query = @"INSERT INTO kullanici (kullaniciAd, kullaniciSifre, kullaniciMail, telefon) VALUES (@ad, @sifre, @mail, @telefon)";
                else
                    query = @"INSERT INTO kullanici (kullaniciAd, kullaniciSifre, kullaniciMail) VALUES (@ad, @sifre, @mail)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ad", kullaniciAd);
                cmd.Parameters.AddWithValue("@sifre", kullaniciSifre);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                if (hasTelefon)
                    cmd.Parameters.AddWithValue("@telefon", telefon ?? string.Empty);
                if (hasProfilResmi)
                    cmd.Parameters.AddWithValue("@profilResmi", DBNull.Value);

                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt hatası: " + ex.ToString(), "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateProfilResmi(string kullaniciMail, string resimPath)
        {
            try
            {
                EnsureProfilResmiColumnExists();
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"UPDATE kullanici SET profilResmi=@resim WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@resim", resimPath ?? string.Empty);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool MailVarMi(string kullaniciMail)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"SELECT COUNT(*) FROM kullanici WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                object result = cmd.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int count)) return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        public bool UpdateTelefon(string kullaniciMail, string telefon)
        {
            try
            {
                EnsureTelefonColumnExists();

                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"UPDATE kullanici SET telefon=@telefon WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@telefon", telefon ?? string.Empty);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool SetAdmin(string kullaniciMail, bool isAdmin)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"UPDATE kullanici SET isAdmin=@isAdmin WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@isAdmin", isAdmin ? 1 : 0);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<Kullanici> GetAllUsers()
        {
            var list = new List<Kullanici>();
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                bool hasProfilResmi = ColumnExists("kullanici", "profilResmi");
                string query = @"SELECT id, kullaniciAd, kullaniciMail, COALESCE(isAdmin,0) as isAdmin, telefon" + (hasProfilResmi ? ", profilResmi" : "") + " FROM kullanici";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int idxId = reader.GetOrdinal("id");
                    int idxAd = reader.GetOrdinal("kullaniciAd");
                    int idxMail = reader.GetOrdinal("kullaniciMail");

                    int idxIsAdmin = -1;
                    if (HasColumn(reader, "isAdmin")) idxIsAdmin = reader.GetOrdinal("isAdmin");

                    int idxTelefon = -1;
                    if (HasColumn(reader, "telefon")) idxTelefon = reader.GetOrdinal("telefon");

                    int idxProfil = -1;
                    if (HasColumn(reader, "profilResmi")) idxProfil = reader.GetOrdinal("profilResmi");

                    var u = new Kullanici
                    {
                        Id = reader.IsDBNull(idxId) ? 0 : reader.GetInt32(idxId),
                        KullaniciAd = reader.IsDBNull(idxAd) ? string.Empty : reader.GetString(idxAd),
                        KullaniciEmail = reader.IsDBNull(idxMail) ? string.Empty : reader.GetString(idxMail),
                        IsAdmin = idxIsAdmin >= 0 && !reader.IsDBNull(idxIsAdmin) && reader.GetInt32(idxIsAdmin) == 1,
                        Telefon = idxTelefon >= 0 && !reader.IsDBNull(idxTelefon) ? reader.GetString(idxTelefon) : null,
                        ProfilResmi = idxProfil >= 0 && !reader.IsDBNull(idxProfil) ? reader.GetString(idxProfil) : null
                    };
                    list.Add(u);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }

        public bool UpdateName(string kullaniciMail, string yeniAd)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"UPDATE kullanici SET kullaniciAd=@ad WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ad", yeniAd ?? string.Empty);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdatePassword(string kullaniciMail, string eskiSifre, string yeniSifre)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string check = @"SELECT kullaniciSifre FROM kullanici WHERE kullaniciMail=@mail LIMIT 1";
                using var cmdCheck = new MySqlCommand(check, conn);
                cmdCheck.Parameters.AddWithValue("@mail", kullaniciMail);
                var obj = cmdCheck.ExecuteScalar();
                if (obj == null) return false;
                var current = obj.ToString();
                if (current != eskiSifre) return false;

                string query = @"UPDATE kullanici SET kullaniciSifre=@yeni WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@yeni", yeniSifre);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DeleteUserByEmail(string kullaniciMail)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string query = @"DELETE FROM kullanici WHERE kullaniciMail=@mail";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@mail", kullaniciMail);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        bool HasColumn(IDataRecord r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
