using System;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using ReStore.Config;
using ReStore.Models;

namespace ReStore.Dao
{
    public class CommentDao
    {
        private readonly string _connStr = DbConfig.ConnectionString;

        private static readonly string[] ProductKeyCandidates = { "product_id", "productId", "urun_id", "UrunID", "urunId" };
        private static readonly string[] UserNameCandidates = { "user_name", "kullanici_adi", "kullaniciAd", "username", "author", "yazan", "kullaniciMail", "kullanici_mail", "email", "mail" };
        private static readonly string[] BodyCandidates = { "comment", "yorum", "yorum_metni", "text", "content", "body", "mesaj", "aciklama", "icerik", "metin" };
        private static readonly string[] CreatedCandidates = { "created_at", "createdAt", "tarih", "olusturma_tarihi" };
        private static readonly string[] IdCandidates = { "id", "comment_id", "yorum_id" };

        private static bool TableExists(MySqlConnection conn, string tableName)
        {
            const string q = @"SELECT COUNT(*) FROM information_schema.TABLES
                WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = LOWER(@t)";
            using var cmd = new MySqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@t", tableName);
            var scalar = cmd.ExecuteScalar();
            return scalar != null && int.TryParse(scalar.ToString(), out int n) && n > 0;
        }

        /// <summary>Veritabanındaki gerçek sütun adını (büyük/küçük harf dahil) döndürür.</summary>
        private static string? FindColumnName(MySqlConnection conn, string tableName, string logicalName)
        {
            const string q = @"SELECT COLUMN_NAME FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = LOWER(@table) AND LOWER(COLUMN_NAME) = LOWER(@col) LIMIT 1";
            using var cmd = new MySqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@table", tableName);
            cmd.Parameters.AddWithValue("@col", logicalName);
            var o = cmd.ExecuteScalar();
            return o?.ToString();
        }

        private static bool ColumnExistsCI(MySqlConnection conn, string tableName, string columnName)
            => FindColumnName(conn, tableName, columnName) != null;

        private static string? ResolveFirst(MySqlConnection conn, string table, string[] candidates)
        {
            foreach (var cand in candidates)
            {
                var actual = FindColumnName(conn, table, cand);
                if (actual != null)
                    return actual;
            }
            return null;
        }

        private static string QIdent(string nameFromDb)
        {
            if (string.IsNullOrEmpty(nameFromDb) || !Regex.IsMatch(nameFromDb, @"^[a-zA-Z0-9_]+$"))
                throw new InvalidOperationException("Geçersiz sütun adı.");
            return "`" + nameFromDb.Replace("`", "") + "`";
        }

        /// <summary>comments tablosunu ve uygulamanın beklediği sütunları yoksa oluşturur / ekler.</summary>
        private static void EnsureCommentsSchema(MySqlConnection conn)
        {
            if (!TableExists(conn, "comments"))
            {
                const string create = @"
CREATE TABLE `comments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `user_id` int unsigned DEFAULT NULL,
  `user_name` varchar(255) NOT NULL DEFAULT '',
  `comment` text NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `ix_comments_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";
                using var cmd = new MySqlCommand(create, conn);
                cmd.ExecuteNonQuery();
                return;
            }

            static void TryAdd(MySqlConnection c, string col, string ddl)
            {
                if (ColumnExistsCI(c, "comments", col))
                    return;
                try
                {
                    using var ac = new MySqlCommand($"ALTER TABLE `comments` ADD COLUMN `{col}` {ddl}", c);
                    ac.ExecuteNonQuery();
                }
                catch
                {
                    /* sütun zaten var veya yetki / motor kısıtı */
                }
            }

            TryAdd(conn, "product_id", "INT NOT NULL DEFAULT 0");
            TryAdd(conn, "user_id", "INT UNSIGNED NULL");
            TryAdd(conn, "user_name", "VARCHAR(255) NULL");
            TryAdd(conn, "comment", "TEXT NULL");
            TryAdd(conn, "created_at", "DATETIME NULL DEFAULT CURRENT_TIMESTAMP");
        }

        private static int? TryGetOrdinal(MySqlDataReader rdr, string columnName)
        {
            try { return rdr.GetOrdinal(columnName); }
            catch (IndexOutOfRangeException) { return null; }
            catch (ArgumentException) { return null; }
        }

        private static string ReadFirstString(MySqlDataReader rdr, string[] actualColumnNamesToTry)
        {
            foreach (var col in actualColumnNamesToTry)
            {
                var ord = TryGetOrdinal(rdr, col);
                if (ord.HasValue && !rdr.IsDBNull(ord.Value))
                    return Convert.ToString(rdr.GetValue(ord.Value)) ?? "";
            }
            return "";
        }

        private static DateTime ReadCreatedAt(MySqlDataReader rdr, string[] dateCols)
        {
            foreach (var col in dateCols)
            {
                var ord = TryGetOrdinal(rdr, col);
                if (ord.HasValue && !rdr.IsDBNull(ord.Value))
                    return Convert.ToDateTime(rdr.GetValue(ord.Value));
            }
            return DateTime.MinValue;
        }

        public List<Comment> GetCommentsByProduct(int productId)
        {
            var list = new List<Comment>();

            using var conn = new MySqlConnection(_connStr);
            conn.Open();
            EnsureCommentsSchema(conn);

            var prodCol = ResolveFirst(conn, "comments", ProductKeyCandidates);
            if (prodCol == null)
                return list;

            string? orderCol = ResolveFirst(conn, "comments", CreatedCandidates)
                ?? ResolveFirst(conn, "comments", IdCandidates);

            string sql = orderCol != null
                ? $"SELECT * FROM `comments` WHERE {QIdent(prodCol)} = @pid ORDER BY {QIdent(orderCol)} DESC"
                : $"SELECT * FROM `comments` WHERE {QIdent(prodCol)} = @pid";

            var nameCols = new System.Collections.Generic.List<string>();
            foreach (var c in UserNameCandidates)
            {
                var a = FindColumnName(conn, "comments", c);
                if (a != null) nameCols.Add(a);
            }
            var bodyCols = new System.Collections.Generic.List<string>();
            foreach (var c in BodyCandidates)
            {
                var a = FindColumnName(conn, "comments", c);
                if (a != null) bodyCols.Add(a);
            }
            var dateCols = new System.Collections.Generic.List<string>();
            foreach (var c in CreatedCandidates)
            {
                var a = FindColumnName(conn, "comments", c);
                if (a != null) dateCols.Add(a);
            }
            var idCols = new System.Collections.Generic.List<string>();
            foreach (var c in IdCandidates)
            {
                var a = FindColumnName(conn, "comments", c);
                if (a != null) idCols.Add(a);
            }
            string? userIdCol = FindColumnName(conn, "comments", "user_id");

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", productId);

            using var rdr = cmd.ExecuteReader();

            var nameArr = nameCols.Count > 0 ? nameCols.ToArray() : new[] { "user_name" };
            var bodyArr = bodyCols.Count > 0 ? bodyCols.ToArray() : new[] { "comment" };
            var dateArr = dateCols.Count > 0 ? dateCols.ToArray() : System.Array.Empty<string>();

            while (rdr.Read())
            {
                int userId = 0;
                if (userIdCol != null)
                {
                    var ordUserId = TryGetOrdinal(rdr, userIdCol);
                    if (ordUserId.HasValue && !rdr.IsDBNull(ordUserId.Value))
                        userId = Convert.ToInt32(rdr.GetValue(ordUserId.Value));
                }

                int id = 0;
                foreach (var realId in idCols)
                {
                    var ordId = TryGetOrdinal(rdr, realId);
                    if (ordId.HasValue && !rdr.IsDBNull(ordId.Value))
                    {
                        id = Convert.ToInt32(rdr.GetValue(ordId.Value));
                        break;
                    }
                }

                var ordPid = TryGetOrdinal(rdr, prodCol);
                int pid = ordPid.HasValue && !rdr.IsDBNull(ordPid.Value) ? Convert.ToInt32(rdr.GetValue(ordPid.Value)) : productId;

                list.Add(new Comment
                {
                    Id = id,
                    ProductId = pid,
                    UserId = userId,
                    UserName = ReadFirstString(rdr, nameArr),
                    Text = ReadFirstString(rdr, bodyArr),
                    CreatedAt = dateArr.Length > 0 ? ReadCreatedAt(rdr, dateArr) : DateTime.MinValue
                });
            }

            return list;
        }

        /// <returns>Baarılıysa true; aksi halde false ve error açıklaması.</returns>
        public bool AddComment(Comment c, out string? error)
        {
            error = null;
            try
            {
                using var conn = new MySqlConnection(_connStr);
                conn.Open();
                EnsureCommentsSchema(conn);

                var prodCol = ResolveFirst(conn, "comments", ProductKeyCandidates);
                var nameCol = ResolveFirst(conn, "comments", UserNameCandidates);
                var bodyCol = ResolveFirst(conn, "comments", BodyCandidates);
                var dateCol = ResolveFirst(conn, "comments", CreatedCandidates);

                if (prodCol == null)
                {
                    error = "comments tablosunda ürün kimliği sütunu (product_id vb.) bulunamadı.";
                    return false;
                }
                if (nameCol == null)
                {
                    error = "comments tablosunda kullanıcı adı sütunu (user_name vb.) bulunamadı.";
                    return false;
                }
                if (bodyCol == null)
                {
                    error = "comments tablosunda yorum metni sütunu (comment, yorum vb.) bulunamadı.";
                    return false;
                }

                bool hasUserId = ColumnExistsCI(conn, "comments", "user_id");

                var cols = new System.Collections.Generic.List<string> { QIdent(prodCol) };
                if (hasUserId)
                    cols.Add(QIdent(FindColumnName(conn, "comments", "user_id")!));
                cols.Add(QIdent(nameCol));
                cols.Add(QIdent(bodyCol));

                var vals = new System.Collections.Generic.List<string> { "@pid" };
                if (hasUserId)
                    vals.Add("@uid");
                vals.Add("@uname");
                vals.Add("@body");

                if (dateCol != null)
                {
                    cols.Add(QIdent(dateCol));
                    vals.Add("@created_at");
                }

                string sql = $"INSERT INTO `comments` ({string.Join(", ", cols)}) VALUES ({string.Join(", ", vals)})";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pid", c.ProductId);
                cmd.Parameters.AddWithValue("@uname", c.UserName ?? "");
                cmd.Parameters.AddWithValue("@body", c.Text ?? "");
                if (dateCol != null)
                    cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
                if (hasUserId)
                    cmd.Parameters.AddWithValue("@uid", c.UserId);

                int n = cmd.ExecuteNonQuery();
                if (n <= 0)
                {
                    error = "Veritabanı kayıt yapmadı (0 satır).";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>Eski çağrılar için — ayrıntılı hata yok.</summary>
        public bool AddComment(Comment c) => AddComment(c, out _);
    }
}
