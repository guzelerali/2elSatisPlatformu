using ReStore.Config;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ReStore.Dao
{
    public class FavoriteDao
    {
        public FavoriteDao()
        {
            EnsureTable();
        }

        private void EnsureTable()
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS favorites (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                userEmail VARCHAR(255) NOT NULL,
                                productId INT NOT NULL
                                )";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Favori tablosu oluşturulurken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool AddFavorite(string userEmail, int productId)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "INSERT INTO favorites (userEmail, productId) VALUES (@user, @pid)";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", userEmail);
                cmd.Parameters.AddWithValue("@pid", productId);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Favori eklenirken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool RemoveFavorite(string userEmail, int productId)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "DELETE FROM favorites WHERE userEmail=@user AND productId=@pid";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", userEmail);
                cmd.Parameters.AddWithValue("@pid", productId);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Favori silinirken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<int> GetFavoritesForUser(string userEmail)
        {
            var list = new List<int>();
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "SELECT productId FROM favorites WHERE userEmail=@user";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@user", userEmail);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetInt32("productId"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Favoriler alınırken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }
    }
}
