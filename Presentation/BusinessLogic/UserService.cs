using DataAccess;
using Models;
using System;
using BusinessLogic.Infrastructure;

namespace BusinessLogic
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        // Uses dependency injection with a default fallback repository
        public UserService(IUserRepository userRepository = null)
        {
            _userRepository = userRepository ?? new UserRepository();
        }

        public User AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            string normalizedUsername = username.Trim();
            UserAuthRecord authRecord = _userRepository.GetUserAuthRecord(normalizedUsername);
            if (authRecord == null || authRecord.User == null)
            {
                ErrorHandler.LogWarning("Authentication failed: user not found for username='" + normalizedUsername + "'");
                throw new InvalidOperationException("Invalid username or password.");
            }

            bool authenticated = false;

            if (!string.IsNullOrWhiteSpace(authRecord.PasswordHash) && !string.IsNullOrWhiteSpace(authRecord.PasswordSalt))
            {
                authenticated = PasswordSecurity.VerifyPassword(password, authRecord.PasswordHash, authRecord.PasswordSalt);
            }
            else if (!string.IsNullOrWhiteSpace(authRecord.LegacyPassword))
            {
                authenticated = string.Equals(authRecord.LegacyPassword, password, StringComparison.Ordinal);
            }

            if (!authenticated)
            {
                ErrorHandler.LogWarning("Authentication failed: invalid password for username='" + normalizedUsername + "'");
                throw new InvalidOperationException("Invalid username or password.");
            }

            return authRecord.User;
        }
    }
}
