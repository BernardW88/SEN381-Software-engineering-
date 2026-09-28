using DataAccess;
using Models;
using System;

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

            User user = _userRepository.GetUserByCredentials(username.Trim(), password);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid username or password.");
            }

            return user;
        }
    }
}