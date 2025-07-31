using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Linq;

namespace PCBTracker.Services
{
    /// <summary>
    /// Provides authentication and user management functionality.
    /// Implements the IUserService interface using Entity Framework Core.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Constructor that accepts a factory for AppDbContext instances.
        /// </summary>
        public UserService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Validates a user's credentials.
        /// Looks up the user by username, then verifies the password hash using BCrypt.
        /// Returns the authenticated User entity if valid; otherwise null.
        /// </summary>
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var user = await db.Users.SingleOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return user;

            return null;
        }

        /// <summary>
        /// Creates and persists a new user account with hashed password and role.
        /// Throws an exception if the username is already in use.
        /// </summary>
        public void CreateUser(int employeeID, string username, string password, string firstName, string lastName, bool admin, bool scan, bool extract, bool edit, bool inspection)
        {
            using var db = _contextFactory.CreateDbContext();

            if (db.Users.Any(u => u.Username == username))
                throw new InvalidOperationException($"User '{username}' already exists.");

            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                EmployeeID = employeeID,
                Username = username,
                PasswordHash = hash,
                FirstName = firstName,
                LastName = lastName,
                Admin = admin,
                Scan = scan,
                Extract = extract,
                Edit = edit,
                Inspection = inspection
            };

            db.Users.Add(user);
            db.SaveChanges();
        }

        public void UpdateUserPermissions(int employeeID, string username, bool admin, bool scan, bool extract, bool edit, bool inspection)
        {
            using var db = _contextFactory.CreateDbContext();

            var user = db.Users.FirstOrDefault(u => u.EmployeeID == employeeID && u.Username == username);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Admin = admin;
            user.Scan = scan;
            user.Extract = extract;
            user.Edit = edit;
            user.Inspection = inspection;

            db.SaveChanges();
        }

        public void RemoveUser(int employeeID, string username)
        {
            using var db = _contextFactory.CreateDbContext();

            if (employeeID == 1 && username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The default admin user cannot be removed.");

            var user = db.Users.FirstOrDefault(u => u.EmployeeID == employeeID && u.Username == username);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            db.Users.Remove(user);
            db.SaveChanges();
        }
    }
}