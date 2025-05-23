using System;
using System.Security.Cryptography;
using System.Text;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Provides AES encryption and decryption services for sensitive data
    /// WARNING: This implementation uses hardcoded key and IV for development/AI-build purposes only.
    /// In production, keys should be securely generated, stored, and managed using proper key management systems.
    /// </summary>
    public static class EncryptionService
    {
        // WARNING: HARDCODED KEY AND IV - FOR DEVELOPMENT ONLY
        // In production, these should be securely generated and stored
        // Consider using Azure Key Vault, AWS KMS, or similar secure key management solutions
        private static readonly byte[] EncryptionKey;
        private static readonly byte[] InitializationVector;

        /// <summary>
        /// Static constructor to initialize encryption key and IV
        /// </summary>
        static EncryptionService()
        {
            // Generate deterministic but pseudo-random key and IV for consistency
            // WARNING: This is NOT secure for production use
            string keySource = "CSharpAIAssistant_SecureKey_Dev_2025";
            string ivSource = "CSharpAI_IV_2025";

            // Ensure key is exactly 32 bytes for AES-256
            EncryptionKey = PadOrTruncateBytes(Encoding.UTF8.GetBytes(keySource), 32);
            
            // Ensure IV is exactly 16 bytes for AES
            InitializationVector = PadOrTruncateBytes(Encoding.UTF8.GetBytes(ivSource), 16);
        }

        /// <summary>
        /// Encrypts plaintext using AES-256-CBC encryption
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>Base64 encoded encrypted string, or original value if null/empty</returns>
        /// <exception cref="CryptographicException">Thrown when encryption fails</exception>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.IV = InitializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Failed to encrypt data", ex);
            }
        }

        /// <summary>
        /// Decrypts Base64 encoded ciphertext using AES-256-CBC decryption
        /// </summary>
        /// <param name="cipherTextBase64">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plaintext, or null if decryption fails</returns>
        public static string Decrypt(string cipherTextBase64)
        {
            if (string.IsNullOrEmpty(cipherTextBase64))
                return cipherTextBase64;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);

                using (var aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.IV = InitializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Trace.TraceError("Invalid Base64 format during decryption: {0}", ex.Message);
                return null;
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Trace.TraceError("Cryptographic error during decryption: {0}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Unexpected error during decryption: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Pads or truncates a byte array to the specified length
        /// </summary>
        /// <param name="source">Source byte array</param>
        /// <param name="targetLength">Target length</param>
        /// <returns>Byte array of exactly the target length</returns>
        private static byte[] PadOrTruncateBytes(byte[] source, int targetLength)
        {
            byte[] result = new byte[targetLength];
            
            if (source.Length >= targetLength)
            {
                // Truncate if source is longer
                Array.Copy(source, result, targetLength);
            }
            else
            {
                // Pad with zeros if source is shorter
                Array.Copy(source, result, source.Length);
                // Remaining bytes are already zero-initialized
            }
            
            return result;
        }

        /// <summary>
        /// Validates that the encryption service is working correctly
        /// </summary>
        /// <returns>True if encryption/decryption round-trip succeeds</returns>
        public static bool ValidateEncryptionService()
        {
            try
            {
                const string testPlaintext = "Test encryption validation 123!@#";
                string encrypted = Encrypt(testPlaintext);
                string decrypted = Decrypt(encrypted);
                
                return testPlaintext.Equals(decrypted, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}
