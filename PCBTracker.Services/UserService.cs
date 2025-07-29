using System;
using System.Linq;
using PCBTracker.Data.Context;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;

namespace PCBTracker.Services
{
    /// <summary>
    /// Provides authentication and user management functionality.
    /// Implements the IUserService interface using Entity Framework Core.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Constructor that accepts an AppDbContext instance for database access.
        /// The context is injected through the DI container at runtime.
        /// </summary>
        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Validates a user's credentials.
        /// Looks up the user by username, then verifies the password hash using BCrypt.
        /// Returns the authenticated User entity if valid; otherwise null.
        /// </summary>
        public User? Authenticate(string username, string password)
        {
            // Look up a single User entity with the matching username.
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == username);

            // If no such user exists, authentication fails.
            if (user == null)
                return null;

            // Compare the supplied password with the stored BCrypt hash.
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user; // Credentials are valid — return user.

            // Password does not match — authentication fails.
            return null;
        }

        /// <summary>
        /// Creates and persists a new user account with hashed password and role.
        /// Throws an exception if the username is already in use.
        /// </summary>
        public void CreateUser(string username, string password, bool admin, bool scan, bool edit, bool inspection)
        {
            // Check for username duplication before proceeding.
            if (_dbContext.Users.Any(u => u.Username == username))
                throw new InvalidOperationException($"User '{username}' already exists.");

            // Hash the password using BCrypt for secure storage.
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create a new User entity with the hashed password and assigned role.
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                Admin = admin,
                Scan = scan,
                Edit= edit,
                Inspection = inspection
            };

            // Add the user to the Users DbSet and persist it to the database.
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
    }
}