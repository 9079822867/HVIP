using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    public static class UserRepository
    {
        // ── Private mapper ────────────────────────────────────
        private static User MapUser(SqlDataReader r)
        {
            return new User
            {
                Id           = (int)r["Id"],
                Name         = r["Name"]    as string ?? "",
                Email        = r["Email"]   as string ?? "",
                Phone        = r["Phone"]   as string ?? "",
                PasswordHash = r["PasswordHash"] as string ?? "",
                Address      = r["Address"] as string ?? "",
                City         = r["City"]    as string ?? "",
                State        = r["State"]   as string ?? "",
                Pincode      = r["Pincode"] as string ?? "",
                RegisteredOn = r["RegisteredOn"] == DBNull.Value
                                  ? DateTime.Now
                                  : (DateTime)r["RegisteredOn"],
                IsAdmin      = r["IsAdmin"]  != DBNull.Value && (bool)r["IsAdmin"],
                IsActive     = SafeBool(r, "IsActive", true)
            };
        }

        private static bool SafeBool(SqlDataReader r, string col, bool defaultVal)
        {
            try { return r[col] != DBNull.Value && (bool)r[col]; }
            catch { return defaultVal; }
        }

        // ── Queries ───────────────────────────────────────────

        public static bool EmailExists(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var parms = new[] { new SqlParameter("@Email", email) };
            return DbHelper.Scalar<int>(sql, parms) > 0;
        }

        public static User ValidateLogin(string email, string password)
        {
            string hash = AuthHelper.HashPassword(password);
            const string sql = @"SELECT TOP 1 * FROM Users
                                 WHERE Email = @Email AND PasswordHash = @Hash";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Hash",  hash);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        var u = MapUser(r);
                        return u.IsActive ? u : null;   // blocked users cannot log in
                    }
                }
            }
            catch { return null; }
        }

        public static User GetById(int id)
        {
            const string sql = "SELECT TOP 1 * FROM Users WHERE Id = @Id";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        return r.Read() ? MapUser(r) : null;
                    }
                }
            }
            catch { return null; }
        }

        // ── Mutations ─────────────────────────────────────────

        public static User Register(string name, string email, string phone, string password)
        {
            if (EmailExists(email)) return null;

            const string sql = @"
                INSERT INTO Users (Name, Email, Phone, PasswordHash, RegisteredOn)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Email, @Phone, @Hash, GETDATE())";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name",  name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hash",  AuthHelper.HashPassword(password));

                    var newId = (int)cmd.ExecuteScalar();
                    return new User
                    {
                        Id           = newId,
                        Name         = name,
                        Email        = email,
                        Phone        = phone ?? "",
                        RegisteredOn = DateTime.Now
                    };
                }
            }
            catch { return null; }
        }

        public static bool UpdateProfile(int userId, string name, string phone,
                                         string address, string city, string state, string pincode)
        {
            const string sql = @"
                UPDATE Users SET
                    Name    = @Name,
                    Phone   = @Phone,
                    Address = @Address,
                    City    = @City,
                    State   = @State,
                    Pincode = @Pincode
                WHERE Id = @Id";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name",    name);
                    cmd.Parameters.AddWithValue("@Phone",   (object)phone    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)address  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City",    (object)city     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@State",   (object)state    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pincode", (object)pincode  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id",      userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch { return false; }
        }

        // ══ Admin methods ══════════════════════════════════════

        public static List<User> GetAllAdmin(int page = 1, int size = 20)
        {
            var list = new List<User>();
            int skip = (page - 1) * size;
            const string sql = @"
                SELECT u.*, ISNULL(o.Cnt,0) AS OrderCount
                FROM Users u
                LEFT JOIN (SELECT UserId, COUNT(1) Cnt FROM Orders GROUP BY UserId) o
                       ON o.UserId = u.Id
                ORDER BY u.RegisteredOn DESC
                OFFSET @Skip ROWS FETCH NEXT @Size ROWS ONLY";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Skip", skip);
                    cmd.Parameters.AddWithValue("@Size", size);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            var u = MapUser(r);
                            u.OrderCount = (int)r["OrderCount"];
                            list.Add(u);
                        }
                }
            }
            catch { }
            return list;
        }

        public static int GetAllAdminCount()
            => DbHelper.Scalar<int>("SELECT COUNT(1) FROM Users");

        public static List<OrderSummary> GetUserOrders(int userId)
        {
            var list = new List<OrderSummary>();
            const string sql = @"
                SELECT o.Id, o.OrderNumber, o.CustomerName, o.Email,
                       o.GrandTotal, o.PaymentMethod, o.Status, o.OrderDate,
                       (SELECT COUNT(1) FROM OrderItems WHERE OrderId=o.Id) AS ItemCount
                FROM Orders o
                WHERE o.UserId = @UserId
                ORDER BY o.OrderDate DESC";
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new OrderSummary
                            {
                                Id            = (int)r["Id"],
                                OrderNumber   = r["OrderNumber"]   as string,
                                CustomerName  = r["CustomerName"]  as string,
                                Email         = r["Email"]         as string,
                                GrandTotal    = (decimal)r["GrandTotal"],
                                PaymentMethod = r["PaymentMethod"] as string,
                                Status        = r["Status"]        as string,
                                OrderDate     = (DateTime)r["OrderDate"],
                                ItemCount     = (int)r["ItemCount"]
                            });
            }
            catch { }
            return list;
        }

        public static bool ToggleActive(int id)
            => DbHelper.Execute(
                "UPDATE Users SET IsActive = CASE WHEN IsActive=1 THEN 0 ELSE 1 END WHERE Id=@Id",
                new[] { new SqlParameter("@Id", id) }) > 0;

        public static bool ToggleAdmin(int id)
            => DbHelper.Execute(
                "UPDATE Users SET IsAdmin = CASE WHEN IsAdmin=1 THEN 0 ELSE 1 END WHERE Id=@Id",
                new[] { new SqlParameter("@Id", id) }) > 0;

        public static bool DeleteUser(int id)
            => DbHelper.Execute("DELETE FROM Users WHERE Id=@Id",
                                new[] { new SqlParameter("@Id", id) }) > 0;
    }
}
