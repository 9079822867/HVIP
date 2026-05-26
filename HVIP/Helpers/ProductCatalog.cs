using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    /// <summary>
    /// Product and Category data access — reads from SQL Server (HVIPDB).
    /// All public method signatures are unchanged so controllers need no edits.
    /// </summary>
    public static class ProductCatalog
    {
        // ══════════════════════════════════════════════════════
        //  Private mappers
        // ══════════════════════════════════════════════════════

        private static Category MapCategory(SqlDataReader r)
        {
            return new Category
            {
                Id          = (int)r["Id"],
                Name        = r["Name"]        as string ?? "",
                Slug        = r["Slug"]        as string ?? "",
                Icon        = r["Icon"]        as string ?? "fas fa-pills",
                Color       = r["Color"]       as string ?? "#1b5e20",
                Description = r["Description"] as string ?? ""
            };
        }

        private static Product MapProduct(SqlDataReader r)
        {
            return new Product
            {
                Id               = (int)r["Id"],
                Name             = r["Name"]             as string ?? "",
                ShortDescription = r["ShortDescription"] as string ?? "",
                Description      = r["Description"]      as string ?? "",
                Price            = (decimal)r["Price"],
                OriginalPrice    = r["OriginalPrice"] == DBNull.Value
                                       ? (decimal?)null
                                       : (decimal)r["OriginalPrice"],
                ImageUrl         = r["ImageUrl"]      as string,
                CategoryId       = (int)r["CategoryId"],
                CategoryName     = r["CategoryName"]  as string ?? "",
                CategoryIcon     = r["CategoryIcon"]  as string ?? "fas fa-pills",
                CategoryColor    = r["CategoryColor"] as string ?? "#1b5e20",
                Brand            = r["Brand"]         as string ?? "HVIP",
                Stock            = (int)r["Stock"],
                IsFeatured       = r["IsFeatured"]   != DBNull.Value && (bool)r["IsFeatured"],
                IsBestseller     = r["IsBestseller"] != DBNull.Value && (bool)r["IsBestseller"],
                IsNew            = r["IsNew"]         != DBNull.Value && (bool)r["IsNew"],
                Rating           = r["Rating"]   == DBNull.Value ? 4.0 : Convert.ToDouble(r["Rating"]),
                ReviewCount      = r["ReviewCount"] == DBNull.Value ? 0 : (int)r["ReviewCount"],
                Size             = r["Size"] as string ?? ""
            };
        }

        // Base SELECT with joined category columns
        private const string ProductSelect = @"
            SELECT p.*, c.Name AS CategoryName, c.Icon AS CategoryIcon, c.Color AS CategoryColor
            FROM   Products  p
            JOIN   Categories c ON c.Id = p.CategoryId
            WHERE  p.IsActive = 1";

        private static List<Product> QueryProducts(string sql, SqlParameter[] parms = null)
        {
            var list = new List<Product>();
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    if (parms != null) cmd.Parameters.AddRange(parms);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(MapProduct(r));
                }
            }
            catch { /* return empty list on DB error */ }
            return list;
        }

        // ══════════════════════════════════════════════════════
        //  Categories
        // ══════════════════════════════════════════════════════

        public static List<Category> GetCategories()
        {
            var list = new List<Category>();
            try
            {
                const string sql = "SELECT * FROM Categories ORDER BY SortOrder, Id";
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                using (var r    = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapCategory(r));
            }
            catch { }
            return list;
        }

        public static Category GetCategory(int id)
        {
            try
            {
                const string sql = "SELECT * FROM Categories WHERE Id = @Id";
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapCategory(r) : null;
                }
            }
            catch { return null; }
        }

        // ══════════════════════════════════════════════════════
        //  Products
        // ══════════════════════════════════════════════════════

        public static List<Product> GetAll()
        {
            return QueryProducts(ProductSelect + " ORDER BY p.Id");
        }

        // Admin versions — no IsActive filter so inactive products are visible/editable
        private const string ProductSelectAdmin = @"
            SELECT p.*, c.Name AS CategoryName, c.Icon AS CategoryIcon, c.Color AS CategoryColor
            FROM   Products  p
            JOIN   Categories c ON c.Id = p.CategoryId";

        public static List<Product> GetAllAdmin()
        {
            return QueryProducts(ProductSelectAdmin + " ORDER BY p.Id");
        }

        public static Product GetByIdAdmin(int id)
        {
            try
            {
                string sql = ProductSelectAdmin + " WHERE p.Id = @Id";
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapProduct(r) : null;
                }
            }
            catch { return null; }
        }

        public static Product GetById(int id)
        {
            try
            {
                string sql = ProductSelect + " AND p.Id = @Id";
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? MapProduct(r) : null;
                }
            }
            catch { return null; }
        }

        public static List<Product> GetFeatured(int count = 8)
        {
            string sql = ProductSelect + $" AND p.IsFeatured = 1 ORDER BY p.Rating DESC, p.ReviewCount DESC OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY";
            return QueryProducts(sql);
        }

        public static List<Product> GetBestsellers(int count = 6)
        {
            string sql = ProductSelect + $" AND p.IsBestseller = 1 ORDER BY p.ReviewCount DESC OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY";
            return QueryProducts(sql);
        }

        public static List<Product> GetNewArrivals(int count = 4)
        {
            string sql = ProductSelect + $" AND p.IsNew = 1 ORDER BY p.Id DESC OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY";
            return QueryProducts(sql);
        }

        public static List<Product> GetByCategory(int categoryId)
        {
            string sql = ProductSelect + " AND p.CategoryId = @CatId ORDER BY p.IsFeatured DESC, p.Rating DESC";
            return QueryProducts(sql, new[] { new SqlParameter("@CatId", categoryId) });
        }

        public static List<Product> GetRelated(int productId, int categoryId, int count = 4)
        {
            string sql = ProductSelect + $" AND p.CategoryId = @CatId AND p.Id <> @PId ORDER BY p.Rating DESC OFFSET 0 ROWS FETCH NEXT {count} ROWS ONLY";
            return QueryProducts(sql, new[]
            {
                new SqlParameter("@CatId", categoryId),
                new SqlParameter("@PId",   productId)
            });
        }

        // ── Search & Filter (used by ShopController) ──────────
        public static List<Product> Search(string keyword = null, int? categoryId = null,
                                           decimal? minPrice = null, decimal? maxPrice = null,
                                           string sortBy = null)
        {
            var where  = "p.IsActive = 1";
            var parms  = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where += " AND (p.Name LIKE @Kw OR p.ShortDescription LIKE @Kw OR p.Brand LIKE @Kw)";
                parms.Add(new SqlParameter("@Kw", "%" + keyword.Trim() + "%"));
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                where += " AND p.CategoryId = @Cat";
                parms.Add(new SqlParameter("@Cat", categoryId.Value));
            }
            if (minPrice.HasValue)
            {
                where += " AND p.Price >= @Min";
                parms.Add(new SqlParameter("@Min", minPrice.Value));
            }
            if (maxPrice.HasValue && maxPrice > 0)
            {
                where += " AND p.Price <= @Max";
                parms.Add(new SqlParameter("@Max", maxPrice.Value));
            }

            string order = "p.Id";
            switch ((sortBy ?? "").ToLower())
            {
                case "price_asc":  order = "p.Price ASC";      break;
                case "price_desc": order = "p.Price DESC";     break;
                case "rating":     order = "p.Rating DESC";    break;
                case "newest":     order = "p.Id DESC";        break;
                default:           order = "p.IsFeatured DESC, p.Rating DESC"; break;
            }

            string sql = $@"
                SELECT p.*, c.Name AS CategoryName, c.Icon AS CategoryIcon, c.Color AS CategoryColor
                FROM   Products   p
                JOIN   Categories c ON c.Id = p.CategoryId
                WHERE  {where}
                ORDER BY {order}";

            return QueryProducts(sql, parms.ToArray());
        }
    }
}
