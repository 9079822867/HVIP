using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    public static class AdminRepository
    {
        // ── Dashboard Stats ───────────────────────────────────
        public static AdminDashboardViewModel GetDashboard()
        {
            var vm = new AdminDashboardViewModel { RecentOrders = new List<OrderSummary>() };
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                {
                    // Products
                    using (var cmd = new SqlCommand("SELECT COUNT(1) TotalP, SUM(CASE WHEN IsActive=1 THEN 1 ELSE 0 END) ActiveP FROM Products", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) { vm.TotalProducts = (int)r["TotalP"]; vm.ActiveProducts = (int)r["ActiveP"]; }
                    }

                    vm.TotalCategories = ExecuteCount(conn, "SELECT COUNT(1) FROM Categories");
                    vm.TotalBanners    = ExecuteCount(conn, "SELECT COUNT(1) FROM Banners");
                    vm.TotalOrders     = ExecuteCount(conn, "SELECT COUNT(1) FROM Orders");
                    vm.TotalUsers      = ExecuteCount(conn, "SELECT COUNT(1) FROM Users");
                    vm.TotalMessages   = ExecuteCount(conn, "SELECT COUNT(1) FROM ContactMessages");
                    vm.UnreadMessages  = ExecuteCount(conn, "SELECT COUNT(1) FROM ContactMessages WHERE IsRead=0");

                    using (var cmd = new SqlCommand("SELECT ISNULL(SUM(GrandTotal),0) FROM Orders", conn))
                        vm.TotalRevenue = Convert.ToDecimal(cmd.ExecuteScalar());

                    // Recent orders
                    const string sql = @"SELECT TOP 10 o.Id,o.OrderNumber,o.CustomerName,o.Email,
                                         o.GrandTotal,o.PaymentMethod,o.Status,o.OrderDate,
                                         (SELECT COUNT(1) FROM OrderItems WHERE OrderId=o.Id) AS ItemCount
                                         FROM Orders o ORDER BY o.OrderDate DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            vm.RecentOrders.Add(new OrderSummary
                            {
                                Id           = (int)r["Id"],
                                OrderNumber  = r["OrderNumber"]  as string,
                                CustomerName = r["CustomerName"] as string,
                                Email        = r["Email"]        as string,
                                GrandTotal   = (decimal)r["GrandTotal"],
                                PaymentMethod= r["PaymentMethod"] as string,
                                Status       = r["Status"] as string,
                                OrderDate    = (DateTime)r["OrderDate"],
                                ItemCount    = (int)r["ItemCount"]
                            });
                }
            }
            catch { }
            return vm;
        }

        private static int ExecuteCount(SqlConnection conn, string sql)
        {
            using (var cmd = new SqlCommand(sql, conn))
                return (int)cmd.ExecuteScalar();
        }

        // ── Product CRUD ──────────────────────────────────────
        public static int SaveProduct(ProductFormViewModel f)
        {
            if (f.Id == 0)
            {
                const string sql = @"INSERT INTO Products
                    (Name,ShortDescription,Description,Price,OriginalPrice,CategoryId,Brand,Stock,Size,
                     IsFeatured,IsBestseller,IsNew,IsActive,Rating,ReviewCount)
                    OUTPUT INSERTED.Id
                    VALUES(@N,@SD,@D,@P,@OP,@Cat,@Br,@St,@Sz,@Feat,@Best,@New,@Act,@R,@RC)";
                return DbHelper.Scalar<int>(sql, ProductParams(f));
            }
            else
            {
                const string sql = @"UPDATE Products SET
                    Name=@N,ShortDescription=@SD,Description=@D,Price=@P,OriginalPrice=@OP,
                    CategoryId=@Cat,Brand=@Br,Stock=@St,Size=@Sz,
                    IsFeatured=@Feat,IsBestseller=@Best,IsNew=@New,IsActive=@Act,Rating=@R,ReviewCount=@RC
                    WHERE Id=@Id";
                DbHelper.Execute(sql, ProductParams(f));
                return f.Id;
            }
        }

        public static bool DeleteProduct(int id)
            => DbHelper.Execute("DELETE FROM Products WHERE Id=@Id",
                                new[] { new SqlParameter("@Id", id) }) > 0;

        public static bool ToggleProduct(int id, string field)
        {
            var allowed = new HashSet<string> { "IsFeatured","IsBestseller","IsNew","IsActive" };
            if (!allowed.Contains(field)) return false;
            return DbHelper.Execute($"UPDATE Products SET {field}=~{field} WHERE Id=@Id",
                                    new[] { new SqlParameter("@Id", id) }) > 0;
        }

        private static SqlParameter[] ProductParams(ProductFormViewModel f) => new[]
        {
            new SqlParameter("@Id",   f.Id),
            new SqlParameter("@N",    f.Name ?? ""),
            new SqlParameter("@SD",   (object)f.ShortDescription ?? DBNull.Value),
            new SqlParameter("@D",    (object)f.Description      ?? DBNull.Value),
            new SqlParameter("@P",    f.Price),
            new SqlParameter("@OP",   (object)f.OriginalPrice    ?? DBNull.Value),
            new SqlParameter("@Cat",  f.CategoryId),
            new SqlParameter("@Br",   f.Brand ?? "HVIP"),
            new SqlParameter("@St",   f.Stock),
            new SqlParameter("@Sz",   (object)f.Size             ?? DBNull.Value),
            new SqlParameter("@Feat", f.IsFeatured),
            new SqlParameter("@Best", f.IsBestseller),
            new SqlParameter("@New",  f.IsNew),
            new SqlParameter("@Act",  f.IsActive),
            new SqlParameter("@R",    (decimal)f.Rating),
            new SqlParameter("@RC",   f.ReviewCount)
        };

        // ── Category CRUD ─────────────────────────────────────
        public static int SaveCategory(Category c)
        {
            if (c.Id == 0)
            {
                const string sql = @"INSERT INTO Categories (Name,Slug,Icon,Color,Description,SortOrder)
                                     OUTPUT INSERTED.Id VALUES(@N,@Sl,@Ic,@Co,@De,@So)";
                return DbHelper.Scalar<int>(sql, CatParams(c));
            }
            else
            {
                const string sql = @"UPDATE Categories SET Name=@N,Slug=@Sl,Icon=@Ic,Color=@Co,Description=@De,SortOrder=@So WHERE Id=@Id";
                DbHelper.Execute(sql, CatParams(c));
                return c.Id;
            }
        }

        public static bool DeleteCategory(int id)
            => DbHelper.Execute("DELETE FROM Categories WHERE Id=@Id",
                                new[] { new SqlParameter("@Id", id) }) > 0;

        private static SqlParameter[] CatParams(Category c) => new[]
        {
            new SqlParameter("@Id", c.Id),
            new SqlParameter("@N",  c.Name  ?? ""),
            new SqlParameter("@Sl", c.Slug  ?? ""),
            new SqlParameter("@Ic", c.Icon  ?? "fas fa-pills"),
            new SqlParameter("@Co", c.Color ?? "#1b5e20"),
            new SqlParameter("@De", (object)c.Description ?? DBNull.Value),
            new SqlParameter("@So", (object)c.SortOrder)
        };

        // ── Orders List ───────────────────────────────────────
        public static List<OrderSummary> GetOrders(int page = 1, int size = 20)
        {
            var list = new List<OrderSummary>();
            int skip = (page - 1) * size;
            const string sql = @"SELECT o.Id,o.OrderNumber,o.CustomerName,o.Email,
                                  o.GrandTotal,o.PaymentMethod,o.Status,o.OrderDate,
                                  (SELECT COUNT(1) FROM OrderItems WHERE OrderId=o.Id) AS ItemCount
                                  FROM Orders o ORDER BY o.OrderDate DESC
                                  OFFSET @Skip ROWS FETCH NEXT @Size ROWS ONLY";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Skip", skip);
                    cmd.Parameters.AddWithValue("@Size", size);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new OrderSummary
                            {
                                Id = (int)r["Id"],
                                OrderNumber = r["OrderNumber"] as string,
                                CustomerName = r["CustomerName"] as string,
                                Email = r["Email"] as string,
                                GrandTotal = (decimal)r["GrandTotal"],
                                PaymentMethod = r["PaymentMethod"] as string,
                                Status = r["Status"] as string,
                                OrderDate = (DateTime)r["OrderDate"],
                                ItemCount = (int)r["ItemCount"]
                            });
                }
            }
            catch { }
            return list;
        }

        public static bool UpdateOrderStatus(int orderId, string status)
            => DbHelper.Execute("UPDATE Orders SET Status=@S WHERE Id=@Id",
                                new[] { new SqlParameter("@S", status), new SqlParameter("@Id", orderId) }) > 0;

        // ── Messages ──────────────────────────────────────────
        public static List<ContactMessage> GetMessages()
        {
            var list = new List<ContactMessage>();
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand("SELECT * FROM ContactMessages ORDER BY ReceivedOn DESC", conn))
                using (var r    = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new ContactMessage
                        {
                            Name    = r["Name"]    as string,
                            Email   = r["Email"]   as string,
                            Phone   = r["Phone"]   as string,
                            Subject = r["Subject"] as string,
                            Message = r["Message"] as string
                        });
            }
            catch { }
            return list;
        }

        public static void MarkMessageRead(int id)
            => DbHelper.Execute("UPDATE ContactMessages SET IsRead=1 WHERE Id=@Id",
                                new[] { new SqlParameter("@Id", id) });
    }
}
