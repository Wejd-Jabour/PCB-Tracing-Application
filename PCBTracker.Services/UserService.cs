// PCBTracker.Services/UserService.cs
using System;
using System.Linq;
using PCBTracker.Data.Context;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;

namespace PCBTracker.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User? Authenticate(string username, string password)
        {
            // 1) Look up the user by username
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
                return null;

            // 2) Verify the submitted password against the stored hash
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user;

            return null;
        }

        public void CreateUser(string username, string password, string role)
        {
            // 1) Prevent duplicate usernames
            if (_dbContext.Users.Any(u => u.Username == username))
                throw new InvalidOperationException($"User '{username}' already exists.");

            // 2) Hash the password before storing
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            // 3) Create and save the new user
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                Role = role
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
    }
}
