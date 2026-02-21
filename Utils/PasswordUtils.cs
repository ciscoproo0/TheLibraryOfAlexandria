using System;

namespace TheLibraryOfAlexandria.Utils
{
    // PasswordUtils class provides utilities for hashing and verifying passwords using bcrypt
    public class PasswordUtils
    {
        // HashPassword method takes a plain text password and returns a bcrypt hash with salt
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            // Use workFactor of 12 for strong hashing (2^12 = 4096 iterations)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // VerifyPassword method checks if a plain text password matches a bcrypt hash
        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            if (string.IsNullOrEmpty(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));

            // BCrypt.Verify automatically handles salt comparison
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
