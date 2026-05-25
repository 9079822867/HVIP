using System;
using System.Collections.Generic;
using System.Linq;
using HVIP.Models;

namespace HVIP.Helpers
{
    public static class UserRepository
    {
        private static readonly object _lock = new object();

        private static readonly List<User> _users = new List<User>
        {
            new User
            {
                Id           = 1,
                Name         = "Demo User",
                Email        = "demo@hvip.com",
                Phone        = "9876543210",
                PasswordHash = AuthHelper.HashPassword("Demo@123"),
                RegisteredOn = new DateTime(2024, 1, 1),
                Address      = "15, Green Park",
                City         = "New Delhi",
                State        = "Delhi",
                Pincode      = "110016"
            }
        };

        private static int _nextId = 2;

        // --- Queries ---
        public static bool EmailExists(string email)
        {
            lock (_lock)
            {
                return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static User ValidateLogin(string email, string password)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (user == null) return null;
                return AuthHelper.VerifyPassword(password, user.PasswordHash) ? user : null;
            }
        }

        public static User GetById(int id)
        {
            lock (_lock) { return _users.FirstOrDefault(u => u.Id == id); }
        }

        // --- Mutations ---
        public static User Register(string name, string email, string phone, string password)
        {
            lock (_lock)
            {
                if (EmailExists(email)) return null;   // duplicate check

                var user = new User
                {
                    Id           = _nextId++,
                    Name         = name,
                    Email        = email,
                    Phone        = phone,
                    PasswordHash = AuthHelper.HashPassword(password),
                    RegisteredOn = DateTime.Now
                };
                _users.Add(user);
                return user;
            }
        }

        public static bool UpdateProfile(int userId, string name, string phone,
                                         string address, string city, string state, string pincode)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return false;

                user.Name    = name;
                user.Phone   = phone;
                user.Address = address;
                user.City    = city;
                user.State   = state;
                user.Pincode = pincode;
                return true;
            }
        }
    }
}
