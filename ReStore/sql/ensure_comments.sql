-- restore veritabanında yorumlar tablosunu uygulamanın beklediği şekle getirir.
-- phpMyAdmin veya mysql istemcisinde restore veritabanını seçip çalıştırın.

USE restore;

CREATE TABLE IF NOT EXISTS `comments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `user_id` int unsigned DEFAULT NULL,
  `user_name` varchar(255) NOT NULL DEFAULT '',
  `comment` text NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `ix_comments_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Eski tabloda eksikse sütun eklemek için (hata verirse sütun zaten vardır, yok sayın):
-- ALTER TABLE `comments` ADD COLUMN `product_id` int NOT NULL DEFAULT 0;
-- ALTER TABLE `comments` ADD COLUMN `user_id` int unsigned DEFAULT NULL;
-- ALTER TABLE `comments` ADD COLUMN `user_name` varchar(255) NULL;
-- ALTER TABLE `comments` ADD COLUMN `comment` text NULL;
-- ALTER TABLE `comments` ADD COLUMN `created_at` datetime NULL DEFAULT CURRENT_TIMESTAMP;
