using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HVIP.Helpers
{
    public static class AuthHelper
    {
        private const string Salt = "HVIP2024_SECURE";
        private const string SessionUserId   = "HVIP_UserId";
        private const string SessionUserName = "HVIP_UserName";
        private const string SessionUserEmail = "HVIP_UserEmail";

        // --- Password hashing ---
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
        {
            return HashPassword(password) == hash;
        }

        // --- Session helpers ---
        public static void SignIn(HttpSessionStateBase session, int userId, string userName, string email)
        {
            session[SessionUserId]    = userId;
            session[SessionUserName]  = userName;
            session[SessionUserEmail] = email;
        }

        public static void SignOut(HttpSessionStateBase session)
        {
            session.Remove(SessionUserId);
            session.Remove(SessionUserName);
            session.Remove(SessionUserEmail);
        }

        public static bool IsLoggedIn(HttpSessionStateBase session)
        {
            return session[SessionUserId] != null;
        }

        public static int GetUserId(HttpSessionStateBase session)
        {
            return session[SessionUserId] != null ? (int)session[SessionUserId] : 0;
        }

        public static string GetUserName(HttpSessionStateBase session)
        {
            return session[SessionUserName] as string ?? string.Empty;
        }

        public static string GetUserEmail(HttpSessionStateBase session)
        {
            return session[SessionUserEmail] as string ?? string.Empty;
        }
    }
}
