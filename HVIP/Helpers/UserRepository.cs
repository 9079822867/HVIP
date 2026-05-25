using System;
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
                                  : (DateTime)r["RegisteredOn"]
            };
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
                        return r.Read() ? MapUser(r) : null;
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
    }
}
