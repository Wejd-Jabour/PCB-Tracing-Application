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
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Username == username);

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
            // Check for username duplication before proceeding.
            if (_dbContext.Users.Any(u => u.Username == username))
                throw new InvalidOperationException($"User '{username}' already exists.");

            // Hash the password using BCrypt for secure storage.
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create a new User entity with the hashed password and assigned role.
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

            // Add the user to the Users DbSet and persist it to the database.
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public void UpdateUserPermissions(int employeeID, string username, bool admin, bool scan, bool extract, bool edit, bool inspection)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.EmployeeID == employeeID && u.Username == username);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Admin = admin;
            user.Scan = scan;
            user.Extract = extract;
            user.Edit = edit;
            user.Inspection = inspection;

            _dbContext.SaveChanges();
        }


        public void RemoveUser(int employeeID, string username)
        {
            // Prevent deletion of default admin
            if (employeeID == 1 && username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The default admin user cannot be removed.");

            var user = _dbContext.Users.FirstOrDefault(u => u.EmployeeID == employeeID && u.Username == username);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }


    }
}