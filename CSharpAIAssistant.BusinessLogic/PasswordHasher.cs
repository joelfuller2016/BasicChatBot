using System;
using System.Security.Cryptography;
using System.Text;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Provides secure password hashing and verification using PBKDF2 (RFC2898)
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltLength = 32; // 256 bits
        private const int HashLength = 32; // 256 bits
        private const int Iterations = 10000; // PBKDF2 iterations

        /// <summary>
        /// Hashes a password using PBKDF2 with a random salt
        /// </summary>
        /// <param name="password">Plain text password to hash</param>
        /// <returns>Base64 encoded string containing salt and hash</returns>
        /// <exception cref="ArgumentException">Thrown when password is null or empty</exception>
        /// <exception cref="CryptographicException">Thrown when hashing fails</exception>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Generate random salt
                byte[] salt = new byte[SaltLength];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Hash password with salt using PBKDF2
                byte[] hash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    hash = pbkdf2.GetBytes(HashLength);
                }

                // Combine salt and hash
                byte[] combined = new byte[SaltLength + HashLength];
                Array.Copy(salt, 0, combined, 0, SaltLength);
                Array.Copy(hash, 0, combined, SaltLength, HashLength);

                // Return as Base64 encoded string
                return Convert.ToBase64String(combined);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Failed to hash password", ex);
            }
        }

        /// <summary>
        /// Verifies a password against a stored salted hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="storedSaltedHash">Base64 encoded salt+hash from storage</param>
        /// <returns>True if password matches, false otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are null or empty</exception>
        public static bool VerifyPassword(string password, string storedSaltedHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(storedSaltedHash))
                throw new ArgumentException("Stored hash cannot be null or empty", nameof(storedSaltedHash));

            try
            {
                // Decode the stored hash
                byte[] combined = Convert.FromBase64String(storedSaltedHash);

                // Verify the combined array has correct length
                if (combined.Length != SaltLength + HashLength)
                {
                    System.Diagnostics.Trace.TraceError("Invalid stored hash format: expected {0} bytes, got {1} bytes", 
                        SaltLength + HashLength, combined.Length);
                    return false;
                }

                // Extract salt and hash
                byte[] salt = new byte[SaltLength];
                byte[] storedHash = new byte[HashLength];
                Array.Copy(combined, 0, salt, 0, SaltLength);
                Array.Copy(combined, SaltLength, storedHash, 0, HashLength);

                // Hash the input password with the extracted salt
                byte[] computedHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    computedHash = pbkdf2.GetBytes(HashLength);
                }

                // Perform constant-time comparison to prevent timing attacks
                return ConstantTimeCompare(storedHash, computedHash);
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Trace.TraceError("Invalid Base64 format in stored hash: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error during password verification: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Performs constant-time comparison of two byte arrays to prevent timing attacks
        /// </summary>
        /// <param name="array1">First array</param>
        /// <param name="array2">Second array</param>
        /// <returns>True if arrays are equal, false otherwise</returns>
        private static bool ConstantTimeCompare(byte[] array1, byte[] array2)
        {
            if (array1 == null || array2 == null)
                return false;

            if (array1.Length != array2.Length)
                return false;

            int result = 0;
            for (int i = 0; i < array1.Length; i++)
            {
                result |= array1[i] ^ array2[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Validates that the password hasher is working correctly
        /// </summary>
        /// <returns>True if hash/verify round-trip succeeds</returns>
        public static bool ValidatePasswordHasher()
        {
            try
            {
                const string testPassword = "TestPassword123!@#";
                const string wrongPassword = "WrongPassword456$%^";

                // Test successful hash and verify
                string hash = HashPassword(testPassword);
                bool correctVerification = VerifyPassword(testPassword, hash);
                bool incorrectVerification = VerifyPassword(wrongPassword, hash);

                // Test that different passwords produce different hashes
                string hash2 = HashPassword(testPassword);

                return correctVerification && 
                       !incorrectVerification && 
                       !string.Equals(hash, hash2, StringComparison.Ordinal); // Should be different due to random salt
            }
            catch
            {
                return false;
            }
        }
    }
}
