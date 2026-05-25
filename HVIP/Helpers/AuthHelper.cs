using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HVIP.Helpers
{
    public static class AuthHelper
    {
        private const string Salt             = "HVIP2024_SECURE";
        private const string SessionUserId    = "HVIP_UserId";
        private const string SessionUserName  = "HVIP_UserName";
        private const string SessionUserEmail = "HVIP_UserEmail";
        private const string SessionIsAdmin   = "HVIP_IsAdmin";

        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var raw   = Salt + password + Salt;
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb    = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;

        public static void SignIn(HttpSessionStateBase session,
                                  int userId, string userName,
                                  string email, bool isAdmin = false)
        {
            session[SessionUserId]    = userId;
            session[SessionUserName]  = userName;
            session[SessionUserEmail] = email;
            session[SessionIsAdmin]   = isAdmin;
        }

        public static void SignOut(HttpSessionStateBase session)
        {
            session.Remove(SessionUserId);
            session.Remove(SessionUserName);
            session.Remove(SessionUserEmail);
            session.Remove(SessionIsAdmin);
        }

        public static bool IsLoggedIn(HttpSessionStateBase session)
            => session[SessionUserId] != null;

        public static bool IsAdmin(HttpSessionStateBase session)
            => session[SessionIsAdmin] is bool b && b;

        public static int    GetUserId   (HttpSessionStateBase session)
            => session[SessionUserId]    != null ? (int)session[SessionUserId] : 0;

        public static string GetUserName (HttpSessionStateBase session)
            => session[SessionUserName]  as string ?? string.Empty;

        public static string GetUserEmail(HttpSessionStateBase session)
            => session[SessionUserEmail] as string ?? string.Empty;
    }
}
