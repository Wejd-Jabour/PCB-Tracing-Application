// PCBTracker.Services/Interfaces/IUserService.cs
using PCBTracker.Domain.Entities;

namespace PCBTracker.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Returns the user if the username/password are valid; otherwise null.
        /// </summary>
        User? Authenticate(string username, string password);

        /// <summary>
        /// Creates a new user with the given password (hashed) and role.
        /// Throws if username already exists.
        /// </summary>
        void CreateUser(string username, string password, string role);
    }
}
