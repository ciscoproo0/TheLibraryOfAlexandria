using System.Security.Cryptography;
using System.Text;

namespace TheLibraryOfAlexandria.Utils
{
    // PasswordUtils class provides utilities for hashing and verifying passwords
    public class PasswordUtils
    {
        // HashPassword method takes a plain text password and returns its SHA256 hash
        public string HashPassword(string password)
        {
            // Using SHA256 cryptographic service provider
            using (var sha256 = SHA256.Create())
            {
                // Compute the hash of the password
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Convert the byte array of the hashed password into a base64 string
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // VerifyPassword method checks if a plain text password matches a previously hashed password
        public bool VerifyPassword(string password, string passwordHash)
        {
            // Using SHA256 cryptographic service provider
            using (var sha256 = SHA256.Create())
            {
                // Compute the hash of the input password
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Convert the byte array of the hashed password into a base64 string
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                // Compare the newly hashed password with the stored hash
                return string.Equals(hashedPassword, passwordHash);
            }
        }
    }
}
