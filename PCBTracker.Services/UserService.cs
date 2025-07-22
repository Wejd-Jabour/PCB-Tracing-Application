// PCBTracker.Services/UserService.cs
using System;
using System.Linq;
using PCBTracker.Data.Context;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;

namespace PCBTracker.Services
{
    /// <summary>
    /// Implements IUserService to handle authentication and user management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// AppDbContext is injected via DI for database operations on Users.
        /// </summary>
        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Authenticates a user by username and password.
        /// Returns the User entity if credentials match; otherwise null.
        /// </summary>
        public User? Authenticate(string username, string password)
        {
            // 1) Look up the user by username in the Users table
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
                return null; // no such user found

            // 2) Verify the submitted password against the stored hash using BCrypt
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user; // authentication successful

            // 3) Password mismatch
            return null;
        }

        /// <summary>
        /// Creates a new user with the specified username, plaintext password, and role.
        /// Throws if the username already exists to prevent duplicates.
        /// </summary>
        public void CreateUser(string username, string password, string role)
        {
            // 1) Ensure the username is unique
            if (_dbContext.Users.Any(u => u.Username == username))
                throw new InvalidOperationException($"User '{username}' already exists.");

            // 2) Hash the plaintext password securely before storing
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            // 3) Create a new User entity with hashed password and role
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                Role = role
            };

            // 4) Persist the new user to the database
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
    }
}
