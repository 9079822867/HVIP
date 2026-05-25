using System.Collections.Generic;
using System.Data.SqlClient;
using HVIP.Models;

namespace HVIP.Helpers
{
    public static class BannerRepository
    {
        private static Banner Map(SqlDataReader r) => new Banner
        {
            Id                = (int)r["Id"],
            Title             = r["Title"]             as string ?? "",
            Subtitle          = r["Subtitle"]          as string ?? "",
            BadgeText         = r["BadgeText"]         as string ?? "",
            Icon              = r["Icon"]              as string ?? "fas fa-leaf",
            BgGradient        = r["BgGradient"]        as string ?? "linear-gradient(135deg,#0d4a1e,#1a6e2e)",
            PrimaryLink       = r["PrimaryLink"]       as string ?? "/Shop",
            PrimaryLinkText   = r["PrimaryLinkText"]   as string ?? "Shop Now",
            SecondaryLink     = r["SecondaryLink"]     as string,
            SecondaryLinkText = r["SecondaryLinkText"] as string,
            SortOrder         = (int)r["SortOrder"],
            IsActive          = r["IsActive"] != System.DBNull.Value && (bool)r["IsActive"]
        };

        public static List<Banner> GetActive()
        {
            var list = new List<Banner>();
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand("SELECT * FROM Banners WHERE IsActive=1 ORDER BY SortOrder", conn))
                using (var r    = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            catch { }
            return list;
        }

        public static List<Banner> GetAll()
        {
            var list = new List<Banner>();
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand("SELECT * FROM Banners ORDER BY SortOrder", conn))
                using (var r    = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            catch { }
            return list;
        }

        public static Banner GetById(int id)
        {
            try
            {
                using (var conn = DbHelper.GetOpenConnection())
                using (var cmd  = new SqlCommand("SELECT * FROM Banners WHERE Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Map(r) : null;
                }
            }
            catch { return null; }
        }

        public static int Save(Banner b)
        {
            if (b.Id == 0)
            {
                const string sql = @"INSERT INTO Banners (Title,Subtitle,BadgeText,Icon,BgGradient,PrimaryLink,PrimaryLinkText,SecondaryLink,SecondaryLinkText,SortOrder,IsActive)
                                     OUTPUT INSERTED.Id VALUES (@T,@Sub,@Badge,@Icon,@Grad,@PL,@PLT,@SL,@SLT,@Sort,@Act)";
                return DbHelper.Scalar<int>(sql, Params(b));
            }
            else
            {
                const string sql = @"UPDATE Banners SET Title=@T,Subtitle=@Sub,BadgeText=@Badge,Icon=@Icon,BgGradient=@Grad,
                                     PrimaryLink=@PL,PrimaryLinkText=@PLT,SecondaryLink=@SL,SecondaryLinkText=@SLT,
                                     SortOrder=@Sort,IsActive=@Act WHERE Id=@Id";
                DbHelper.Execute(sql, Params(b));
                return b.Id;
            }
        }

        public static void Delete(int id)
            => DbHelper.Execute("DELETE FROM Banners WHERE Id=@Id",
                                new[] { new SqlParameter("@Id", id) });

        private static SqlParameter[] Params(Banner b) => new[]
        {
            new SqlParameter("@Id",   b.Id),
            new SqlParameter("@T",    b.Title ?? ""),
            new SqlParameter("@Sub",  (object)b.Subtitle          ?? System.DBNull.Value),
            new SqlParameter("@Badge",(object)b.BadgeText         ?? System.DBNull.Value),
            new SqlParameter("@Icon", (object)b.Icon              ?? System.DBNull.Value),
            new SqlParameter("@Grad", b.BgGradient ?? "linear-gradient(135deg,#0d4a1e,#1a6e2e)"),
            new SqlParameter("@PL",   (object)b.PrimaryLink       ?? System.DBNull.Value),
            new SqlParameter("@PLT",  (object)b.PrimaryLinkText   ?? System.DBNull.Value),
            new SqlParameter("@SL",   (object)b.SecondaryLink     ?? System.DBNull.Value),
            new SqlParameter("@SLT",  (object)b.SecondaryLinkText ?? System.DBNull.Value),
            new SqlParameter("@Sort", b.SortOrder),
            new SqlParameter("@Act",  b.IsActive)
        };
    }
}
