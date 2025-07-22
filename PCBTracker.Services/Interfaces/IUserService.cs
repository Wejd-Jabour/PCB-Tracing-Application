using PCBTracker.Domain.Entities;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Defines user authentication and management operations.
    /// Allows ViewModels and other services to authenticate users and create new accounts
    /// without needing to know the underlying data access details.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Validates the given username and password.
        /// Returns the matching User entity if credentials are correct, or null if invalid.
        /// </summary>
        /// <param name="username">The user's login name.</param>
        /// <param name="password">The plain-text password to verify against the stored hash.</param>
        /// <returns>User entity on success, or null on failure.</returns>
        User? Authenticate(string username, string password);

        /// <summary>
        /// Creates a new user account with the specified role.
        /// Throws if a user with the same username already exists.
        /// </summary>
        /// <param name="username">Desired username (must be unique).</param>
        /// <param name="password">Plain-text password; will be hashed before storage.</param>
        /// <param name="role">Role assignment for the user, e.g. "Admin" or "Standard".</param>
        void CreateUser(string username, string password, string role);
    }
}
