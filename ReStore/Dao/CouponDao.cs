using ReStore.Config;
using ReStore.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ReStore.Dao
{
    public class CouponDao
    {
        public CouponDao()
        {
            EnsureTable();
            EnsureUserCouponsTable();
            EnsureAssignmentLogTable();
        }

        public bool AssignCouponToAllUsers(string couponCode)
        {
            try
            {
                EnsureUserCouponsTable();
                EnsureAssignmentLogTable();
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                // get all user emails
                string q = "SELECT kullaniciMail FROM kullanici";
                using var cmd = new MySqlCommand(q, conn);
                using var reader = cmd.ExecuteReader();
                var emails = new List<string>();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0)) emails.Add(reader.GetString(0));
                }
                reader.Close();

                using var tran = conn.BeginTransaction();
                using var insertCmd = new MySqlCommand("INSERT IGNORE INTO user_coupons (user_email, coupon_code) VALUES (@mail,@code)", conn, tran);
                insertCmd.Parameters.Add(new MySqlParameter("@mail", ""));
                insertCmd.Parameters.AddWithValue("@code", couponCode);
                using var logCmd = new MySqlCommand("INSERT INTO coupon_assign_log (batch_id, coupon_code, user_email, assigned_at) VALUES (@batch,@code,@mail,@at)", conn, tran);
                logCmd.Parameters.AddWithValue("@batch", Guid.NewGuid().ToString());
                logCmd.Parameters.AddWithValue("@code", couponCode);
                logCmd.Parameters.Add(new MySqlParameter("@mail", ""));
                logCmd.Parameters.AddWithValue("@at", DateTime.Now);
                foreach (var e in emails)
                {
                    insertCmd.Parameters["@mail"].Value = e;
                    insertCmd.ExecuteNonQuery();
                    try
                    {
                        logCmd.Parameters["@mail"].Value = e;
                        logCmd.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                }
                tran.Commit();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tüm kullanıcılara kupon atama hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void EnsureUserCouponsTable()
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS user_coupons (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                user_email VARCHAR(255) NOT NULL,
                                coupon_code VARCHAR(100) NOT NULL,
                                UNIQUE KEY uq_user_coupon (user_email, coupon_code)
                                )";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        private void EnsureAssignmentLogTable()
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS coupon_assign_log (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                batch_id VARCHAR(100) NOT NULL,
                                coupon_code VARCHAR(100) NOT NULL,
                                user_email VARCHAR(255) NOT NULL,
                                assigned_at DATETIME NOT NULL
                                )";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public List<Coupon> GetAll()
        {
            var list = new List<Coupon>();
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "SELECT id, code, percent, amount, expiresAt, active FROM coupons ORDER BY id DESC";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Coupon
                    {
                        Id = reader.GetInt32("id"),
                        Code = reader.IsDBNull(reader.GetOrdinal("code")) ? string.Empty : reader.GetString("code"),
                        Percent = reader.IsDBNull(reader.GetOrdinal("percent")) ? (decimal?)null : reader.GetDecimal("percent"),
                        Amount = reader.IsDBNull(reader.GetOrdinal("amount")) ? (decimal?)null : reader.GetDecimal("amount"),
                        ExpiresAt = reader.IsDBNull(reader.GetOrdinal("expiresAt")) ? (DateTime?)null : reader.GetDateTime("expiresAt"),
                        Active = !reader.IsDBNull(reader.GetOrdinal("active")) && reader.GetInt32("active") == 1
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kuponlar alınırken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }

        private void EnsureTable()
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS coupons (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                code VARCHAR(100) NOT NULL UNIQUE,
                                percent DECIMAL(5,2) NULL,
                                amount DECIMAL(10,2) NULL,
                                expiresAt DATETIME NULL,
                                active TINYINT(1) NOT NULL DEFAULT 1
                                )";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon tablosu oluşturulurken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool AddCoupon(Coupon c)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "INSERT INTO coupons (code, percent, amount, expiresAt, active) VALUES (@code,@percent,@amount,@exp,@active)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", c.Code);
                cmd.Parameters.AddWithValue("@percent", c.Percent.HasValue ? (object)c.Percent.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@amount", c.Amount.HasValue ? (object)c.Amount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@exp", c.ExpiresAt.HasValue ? (object)c.ExpiresAt.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@active", c.Active ? 1 : 0);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon eklenirken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool AssignCouponToUser(string userEmail, string couponCode)
        {
            try
            {
                EnsureUserCouponsTable();
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "INSERT IGNORE INTO user_coupons (user_email, coupon_code) VALUES (@mail,@code)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@mail", userEmail);
                cmd.Parameters.AddWithValue("@code", couponCode);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon atama hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<Coupon> GetCouponsForUser(string userEmail)
        {
            var list = new List<Coupon>();
            try
            {
                EnsureUserCouponsTable();
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = @"SELECT c.id, c.code, c.percent, c.amount, c.expiresAt, c.active
                               FROM coupons c
                               JOIN user_coupons uc ON uc.coupon_code = c.code
                               WHERE uc.user_email = @mail
                               ORDER BY c.id DESC";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@mail", userEmail);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Coupon
                    {
                        Id = reader.GetInt32("id"),
                        Code = reader.IsDBNull(reader.GetOrdinal("code")) ? string.Empty : reader.GetString("code"),
                        Percent = reader.IsDBNull(reader.GetOrdinal("percent")) ? (decimal?)null : reader.GetDecimal("percent"),
                        Amount = reader.IsDBNull(reader.GetOrdinal("amount")) ? (decimal?)null : reader.GetDecimal("amount"),
                        ExpiresAt = reader.IsDBNull(reader.GetOrdinal("expiresAt")) ? (DateTime?)null : reader.GetDateTime("expiresAt"),
                        Active = !reader.IsDBNull(reader.GetOrdinal("active")) && reader.GetInt32("active") == 1
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kuponlar alınırken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }

        public Coupon? GetByCode(string code)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "SELECT id, code, percent, amount, expiresAt, active FROM coupons WHERE code=@code LIMIT 1";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Coupon
                    {
                        Id = reader.GetInt32("id"),
                        Code = reader.IsDBNull(reader.GetOrdinal("code")) ? string.Empty : reader.GetString("code"),
                        Percent = reader.IsDBNull(reader.GetOrdinal("percent")) ? (decimal?)null : reader.GetDecimal("percent"),
                        Amount = reader.IsDBNull(reader.GetOrdinal("amount")) ? (decimal?)null : reader.GetDecimal("amount"),
                        ExpiresAt = reader.IsDBNull(reader.GetOrdinal("expiresAt")) ? (DateTime?)null : reader.GetDateTime("expiresAt"),
                        Active = !reader.IsDBNull(reader.GetOrdinal("active")) && reader.GetInt32("active") == 1
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon alınırken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public bool DeleteCouponByCode(string code)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "DELETE FROM coupons WHERE code=@code";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon silme hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateCoupon(Coupon c)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
  
                string? oldCode = null;
                using (var getCmd = new MySqlCommand("SELECT code FROM coupons WHERE id=@id LIMIT 1", conn))
                {
                    getCmd.Parameters.AddWithValue("@id", c.Id);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        oldCode = reader.IsDBNull(reader.GetOrdinal("code")) ? string.Empty : reader.GetString("code");
                    }
                    reader.Close();
                }

                string sql = "UPDATE coupons SET code=@code, percent=@percent, amount=@amount, expiresAt=@exp, active=@active WHERE id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", c.Code);
                cmd.Parameters.AddWithValue("@percent", c.Percent.HasValue ? (object)c.Percent.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@amount", c.Amount.HasValue ? (object)c.Amount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@exp", c.ExpiresAt.HasValue ? (object)c.ExpiresAt.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@active", c.Active ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", c.Id);
                int affected = cmd.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(oldCode) && !string.Equals(oldCode, c.Code, StringComparison.Ordinal))
                {
                    try
                    {
                        using var upd = new MySqlCommand("UPDATE user_coupons SET coupon_code=@new WHERE coupon_code=@old", conn);
                        upd.Parameters.AddWithValue("@new", c.Code);
                        upd.Parameters.AddWithValue("@old", oldCode);
                        upd.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                }

                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kupon güncelleme hatası: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
