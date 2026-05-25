using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HVIP.Helpers
{
    public static class DbHelper
    {
        private static string _connString;

        private static string ConnString
        {
            get
            {
                if (_connString == null)
                    _connString = ConfigurationManager
                                    .ConnectionStrings["sqlconn"]
                                    .ConnectionString;
                return _connString;
            }
        }

        /// <summary>Returns an open SqlConnection. Caller must dispose.</summary>
        public static SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(ConnString);
            conn.Open();
            return conn;
        }

        /// <summary>Returns a closed SqlConnection. Caller opens and disposes.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnString);
        }

        /// <summary>Quick scalar helper — returns default(T) on null/DBNull.</summary>
        public static T Scalar<T>(string sql, SqlParameter[] parms = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                var result = cmd.ExecuteScalar();
                if (result == null || result == System.DBNull.Value) return default(T);
                return (T)result;
            }
        }

        /// <summary>Quick non-query — returns rows affected.</summary>
        public static int Execute(string sql, SqlParameter[] parms = null)
        {
            using (var conn = GetOpenConnection())
            using (var cmd  = new SqlCommand(sql, conn))
            {
                if (parms != null) cmd.Parameters.AddRange(parms);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
