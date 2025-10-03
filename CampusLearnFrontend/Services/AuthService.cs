using CampusLearnFrontend.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CampusLearnFrontend.Services
{
    public class AuthService
    {
        private readonly CustomAuthStateProvider _authStateProvider;
        private List<User> _users = new();
        private User? _currentUser;

        public AuthService(CustomAuthStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public bool Register(User user)
        {
            if (_users.Any(u => u.Email == user.Email))
                return false;

            _users.Add(user);
            return true;
        }

        public bool Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                _currentUser = user;
                _authStateProvider.NotifyAuthStateChanged();
                return true;
            }
            return false;
        }

        public User? GetCurrentUser() => _currentUser;

        public void Logout()
        {
            _currentUser = null;
            _authStateProvider.NotifyAuthStateChanged();
        }
    }
}
