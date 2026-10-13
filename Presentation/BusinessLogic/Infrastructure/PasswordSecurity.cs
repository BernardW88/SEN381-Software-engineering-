using System;
using System.Security.Cryptography;

namespace BusinessLogic.Infrastructure
{
    public static class PasswordSecurity
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static (string HashBase64, string SaltBase64) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool VerifyPassword(string password, string expectedHashBase64, string saltBase64)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(expectedHashBase64) || string.IsNullOrWhiteSpace(saltBase64))
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(saltBase64);
                byte[] expectedHash = Convert.FromBase64String(expectedHashBase64);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
