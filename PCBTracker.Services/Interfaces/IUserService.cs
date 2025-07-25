﻿using PCBTracker.Domain.Entities;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Declares methods for user authentication and account creation.
    /// Enables authentication workflows without exposing data access logic.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user based on submitted credentials.
        /// Compares the provided plain-text password against the stored password hash.
        /// </summary>
        /// <param name="username">The login name of the user to verify.</param>
        /// <param name="password">The plain-text password provided at login.</param>
        /// <returns>
        /// A User entity if the credentials match a stored user; otherwise, null.
        /// </returns>
        User? Authenticate(string username, string password);

        /// <summary>
        /// Creates a new user record with a specified role and securely hashed password.
        /// If a user with the same username already exists, the method throws.
        /// </summary>
        /// <param name="username">The unique login name for the new user.</param>
        /// <param name="password">The plain-text password to be hashed and stored securely.</param>
        /// <param name="role">The user's assigned role. This value is stored as a string.</param>
        void CreateUser(string username, string password, string role);
    }
}
