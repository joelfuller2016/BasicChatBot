using System;
using System.Data.SQLite;
using System.Diagnostics;
using CSharpAIAssistant.BusinessLogic;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Handles seeding of initial data when the database is first created
    /// </summary>
    public static class DataSeeder
    {
        /// <summary>
        /// Seeds initial data including admin user and default application settings
        /// </summary>
        /// <param name="connection">Open SQLite connection</param>
        /// <param name="transaction">Active SQLite transaction</param>
        public static void SeedInitialData(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            try
            {
                SeedAdminUser(connection, transaction);
                SeedApplicationSettings(connection, transaction);
                
                Trace.WriteLine("Initial data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error during data seeding: {0}", ex.ToString());
                throw;
            }
        }

        private static void SeedAdminUser(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            // Check if Users table is empty
            string countQuery = "SELECT COUNT(*) FROM Users";
            using (var command = new SQLiteCommand(countQuery, connection, transaction))
            {
                long userCount = (long)command.ExecuteScalar();
                
                if (userCount == 0)
                {
                    // Insert default admin user with properly hashed password
                    string insertUserQuery = @"
                        INSERT INTO Users (Username, Email, PasswordHash, IsAdmin, RegistrationDate) 
                        VALUES (@Username, @Email, @PasswordHash, @IsAdmin, @RegistrationDate)";
                    
                    using (var insertCommand = new SQLiteCommand(insertUserQuery, connection, transaction))
                    {
                        const string defaultPassword = "adminpassword";
                        string hashedPassword = PasswordHasher.HashPassword(defaultPassword);
                        
                        insertCommand.Parameters.AddWithValue("@Username", "admin");
                        insertCommand.Parameters.AddWithValue("@Email", "admin@example.com");
                        insertCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        insertCommand.Parameters.AddWithValue("@IsAdmin", 1);
                        insertCommand.Parameters.AddWithValue("@RegistrationDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                        
                        insertCommand.ExecuteNonQuery();
                    }
                    
                    Trace.WriteLine("Default admin user created: admin/adminpassword (Default password should be changed immediately after first login)");
                }
                else
                {
                    Trace.WriteLine("Users table is not empty. Skipping admin user creation.");
                }
            }
        }

        private static void SeedApplicationSettings(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            // Check if ApplicationSettings table is empty
            string countQuery = "SELECT COUNT(*) FROM ApplicationSettings";
            using (var command = new SQLiteCommand(countQuery, connection, transaction))
            {
                long settingsCount = (long)command.ExecuteScalar();
                
                if (settingsCount == 0)
                {
                    // Define default settings with encrypted placeholders where appropriate
                    var defaultSettings = new[]
                    {
                        new { Key = "GoogleClientId", Value = "YOUR_GOOGLE_CLIENT_ID_HERE", Description = "Google OAuth Client ID", DataType = "String", IsSensitive = 0, GroupName = "Authentication" },
                        new { Key = "GoogleClientSecret_Encrypted", Value = GetEncryptedPlaceholder("YOUR_GOOGLE_CLIENT_SECRET_HERE"), Description = "Google OAuth Client Secret (Encrypted)", DataType = "EncryptedString", IsSensitive = 1, GroupName = "Authentication" },
                        new { Key = "OpenAIApiKey_Default_Encrypted", Value = GetEncryptedPlaceholder("YOUR_OPENAI_API_KEY_HERE"), Description = "Default OpenAI API Key (Encrypted)", DataType = "EncryptedString", IsSensitive = 1, GroupName = "AI" },
                        new { Key = "SessionTimeoutMinutes", Value = "30", Description = "Session timeout in minutes", DataType = "Integer", IsSensitive = 0, GroupName = "Security" },
                        new { Key = "DefaultAIModelIdentifier", Value = "gpt-3.5-turbo", Description = "Default AI model identifier", DataType = "String", IsSensitive = 0, GroupName = "AI" },
                        new { Key = "DefaultAITaskMaxTokens", Value = "1000", Description = "Default maximum tokens for AI tasks", DataType = "Integer", IsSensitive = 0, GroupName = "AI" },
                        new { Key = "DefaultAITaskTemperature", Value = "0.7", Description = "Default temperature for AI tasks", DataType = "Real", IsSensitive = 0, GroupName = "AI" },
                        new { Key = "UseMockAIService", Value = "true", Description = "Whether to use mock AI service for testing", DataType = "Boolean", IsSensitive = 0, GroupName = "Development" }
                    };

                    string insertSettingQuery = @"
                        INSERT INTO ApplicationSettings (SettingKey, SettingValue, SettingDescription, DataType, IsSensitive, GroupName, CreatedAt) 
                        VALUES (@SettingKey, @SettingValue, @SettingDescription, @DataType, @IsSensitive, @GroupName, @CreatedAt)";
                    
                    foreach (var setting in defaultSettings)
                    {
                        using (var insertCommand = new SQLiteCommand(insertSettingQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@SettingKey", setting.Key);
                            insertCommand.Parameters.AddWithValue("@SettingValue", setting.Value);
                            insertCommand.Parameters.AddWithValue("@SettingDescription", setting.Description);
                            insertCommand.Parameters.AddWithValue("@DataType", setting.DataType);
                            insertCommand.Parameters.AddWithValue("@IsSensitive", setting.IsSensitive);
                            insertCommand.Parameters.AddWithValue("@GroupName", setting.GroupName);
                            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                            
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    
                    Trace.WriteLine("Default application settings seeded. Count: {0} (Encrypted values are properly encrypted placeholders)", defaultSettings.Length);
                }
                else
                {
                    Trace.WriteLine("ApplicationSettings table is not empty. Skipping settings seeding.");
                }
            }
        }

        /// <summary>
        /// Encrypts a placeholder value for seeded sensitive settings
        /// </summary>
        /// <param name="placeholder">Placeholder text to encrypt</param>
        /// <returns>Encrypted placeholder value</returns>
        private static string GetEncryptedPlaceholder(string placeholder)
        {
            try
            {
                return EncryptionService.Encrypt(placeholder);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to encrypt placeholder '{0}': {1}", placeholder, ex.Message);
                return "ENCRYPTION_FAILED_" + placeholder;
            }
        }
    }
}
