using ReStore.Config;
using ReStore.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ReStore.Dao
{
    public class ProductDao
    {
        public ProductDao()
        {
            EnsureTables();
        }

        private void EnsureTables()
        {
            try
            {
                using (var conn = new MySqlConnection(DbConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = @"
CREATE TABLE IF NOT EXISTS products (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  price DECIMAL(10,2) NOT NULL,
  sellerEmail VARCHAR(255) NOT NULL,
  imagePath VARCHAR(1024),
  category VARCHAR(100) NULL,
  gender VARCHAR(50) NULL,
  brand VARCHAR(100) NULL,
  size VARCHAR(50) NULL
)";
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    try
                    {
                        using var alt = new MySqlCommand("ALTER TABLE products ADD COLUMN category VARCHAR(100) NULL", conn);
                        alt.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                    try
                    {
                        using var alt = new MySqlCommand("ALTER TABLE products ADD COLUMN gender VARCHAR(50) NULL", conn);
                        alt.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                    try
                    {
                        using var alt = new MySqlCommand("ALTER TABLE products ADD COLUMN brand VARCHAR(100) NULL", conn);
                        alt.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                    try
                    {
                        using var alt = new MySqlCommand("ALTER TABLE products ADD COLUMN size VARCHAR(50) NULL", conn);
                        alt.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün tablosu oluşturulurken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool AddProduct(Product p)
        {
            try
            {
                using (var conn = new MySqlConnection(DbConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO products (title, description, price, sellerEmail, imagePath, category, gender, brand, size)
                                   VALUES (@title, @desc, @price, @seller, @img, @category, @gender, @brand, @size)";
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@title", p.Title);
                    cmd.Parameters.AddWithValue("@desc", p.Description);
                    cmd.Parameters.AddWithValue("@price", p.Price);
                    cmd.Parameters.AddWithValue("@seller", p.SellerEmail);
                    cmd.Parameters.AddWithValue("@img", p.ImagePath ?? string.Empty);
                    cmd.Parameters.AddWithValue("@category", p.Category ?? string.Empty);
                    cmd.Parameters.AddWithValue("@gender", p.Gender ?? string.Empty);
                    cmd.Parameters.AddWithValue("@brand", p.Brand ?? string.Empty);
                    cmd.Parameters.AddWithValue("@size", p.Size ?? string.Empty);
                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün eklenirken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<Product> GetAllProducts()
        {
            var list = new List<Product>();
            try
            {
                using (var conn = new MySqlConnection(DbConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT id, title, description, price, sellerEmail, imagePath, category, gender, brand, size FROM products ORDER BY id DESC";
                    using var cmd = new MySqlCommand(sql, conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new Product
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.IsDBNull(reader.GetOrdinal("title")) ? string.Empty : reader.GetString("title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                            Price = reader.IsDBNull(reader.GetOrdinal("price")) ? 0 : reader.GetDecimal("price"),
                            SellerEmail = reader.IsDBNull(reader.GetOrdinal("sellerEmail")) ? string.Empty : reader.GetString("sellerEmail"),
                            ImagePath = reader.IsDBNull(reader.GetOrdinal("imagePath")) ? string.Empty : reader.GetString("imagePath"),
                            Category = reader.IsDBNull(reader.GetOrdinal("category")) ? string.Empty : reader.GetString("category"),
                            Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? string.Empty : reader.GetString("gender"),
                            Brand = reader.IsDBNull(reader.GetOrdinal("brand")) ? string.Empty : reader.GetString("brand"),
                            Size = reader.IsDBNull(reader.GetOrdinal("size")) ? string.Empty : reader.GetString("size")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler alınırken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }

        public bool DeleteProduct(int id)
        {
            try
            {
                using var conn = new MySqlConnection(DbConfig.ConnectionString);
                conn.Open();
                string sql = "DELETE FROM products WHERE id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün silinirken hata: " + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
