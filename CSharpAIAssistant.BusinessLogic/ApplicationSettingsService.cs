using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Caching;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Business logic service for managing application settings
    /// Provides caching, encryption/decryption, and typed value retrieval
    /// </summary>
    public class ApplicationSettingsService
    {
        private readonly ApplicationSettingsDAL _settingsDAL;
        private const string CacheKeyPrefix = "AppSetting_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Initializes a new instance with default ApplicationSettingsDAL
        /// </summary>
        public ApplicationSettingsService()
        {
            _settingsDAL = new ApplicationSettingsDAL();
        }

        /// <summary>
        /// Initializes a new instance with provided ApplicationSettingsDAL for dependency injection
        /// </summary>
        /// <param name="settingsDAL">ApplicationSettingsDAL instance</param>
        public ApplicationSettingsService(ApplicationSettingsDAL settingsDAL)
        {
            _settingsDAL = settingsDAL ?? throw new ArgumentNullException(nameof(settingsDAL));
        }

        /// <summary>
        /// Gets a setting value as string, with caching for non-sensitive settings
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found</param>
        /// <returns>Setting value or default value</returns>
        public string GetSettingValue(string key, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            try
            {
                // Try cache first for non-sensitive settings
                string cacheKey = CacheKeyPrefix + key;
                if (HttpContext.Current?.Cache != null)
                {
                    var cachedValue = HttpContext.Current.Cache[cacheKey] as string;
                    if (cachedValue != null)
                        return cachedValue;
                }

                // Fetch from database
                var setting = _settingsDAL.GetSettingByKey(key);
                if (setting == null)
                    return defaultValue;

                // Don't return sensitive values through generic getter
                if (setting.IsSensitive)
                    return defaultValue;

                // Cache non-sensitive values
                if (HttpContext.Current?.Cache != null && !setting.IsSensitive)
                {
                    HttpContext.Current.Cache.Insert(cacheKey, setting.SettingValue, 
                        null, DateTime.Now.Add(CacheDuration), TimeSpan.Zero);
                }

                return setting.SettingValue ?? defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving setting '{0}': {1}", key, ex.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a decrypted setting value for sensitive/encrypted settings
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or decryption fails</param>
        /// <returns>Decrypted setting value or default value</returns>
        public string GetDecryptedSettingValue(string key, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            try
            {
                var setting = _settingsDAL.GetSettingByKey(key);
                if (setting == null)
                    return defaultValue;

                // If setting is sensitive and encrypted
                if (setting.IsSensitive && 
                    string.Equals(setting.DataType, "EncryptedString", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(setting.SettingValue))
                {
                    string decrypted = EncryptionService.Decrypt(setting.SettingValue);
                    return decrypted ?? defaultValue;
                }

                // Return plain value for non-encrypted settings
                return setting.SettingValue ?? defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving decrypted setting '{0}': {1}", key, ex.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a setting value as boolean
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or invalid</param>
        /// <returns>Boolean setting value or default</returns>
        public bool GetBooleanSettingValue(string key, bool defaultValue = false)
        {
            string value = GetSettingValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (bool.TryParse(value, out bool result))
                return result;

            // Handle common string representations
            string lowerValue = value.ToLowerInvariant();
            if (lowerValue == "1" || lowerValue == "yes" || lowerValue == "on")
                return true;
            if (lowerValue == "0" || lowerValue == "no" || lowerValue == "off")
                return false;

            return defaultValue;
        }

        /// <summary>
        /// Gets a setting value as integer
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or invalid</param>
        /// <returns>Integer setting value or default</returns>
        public int GetIntegerSettingValue(string key, int defaultValue = 0)
        {
            string value = GetSettingValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (int.TryParse(value, out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Gets a setting value as double
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or invalid</param>
        /// <returns>Double setting value or default</returns>
        public double GetRealSettingValue(string key, double defaultValue = 0.0)
        {
            string value = GetSettingValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Saves or updates an application setting, handling encryption for sensitive values
        /// </summary>
        /// <param name="settingFromUI">ApplicationSetting from UI with updated values</param>
        /// <exception cref="ArgumentNullException">Thrown when settingFromUI is null</exception>
        public void SaveSetting(ApplicationSetting settingFromUI)
        {
            if (settingFromUI == null)
                throw new ArgumentNullException(nameof(settingFromUI));

            try
            {
                // Get existing setting
                var existingSetting = _settingsDAL.GetSettingByKey(settingFromUI.SettingKey);
                bool isUpdate = existingSetting != null;

                // Prepare setting for save
                var settingToSave = isUpdate ? existingSetting : new ApplicationSetting();
                
                // Update properties
                settingToSave.SettingKey = settingFromUI.SettingKey;
                settingToSave.SettingDescription = settingFromUI.SettingDescription;
                settingToSave.DataType = settingFromUI.DataType ?? "String";
                settingToSave.IsSensitive = settingFromUI.IsSensitive;
                settingToSave.GroupName = settingFromUI.GroupName;

                // Handle value encryption for sensitive settings
                if (settingFromUI.IsSensitive && 
                    string.Equals(settingFromUI.DataType, "EncryptedString", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(settingFromUI.SettingValue))
                {
                    // Check if this is a placeholder indicating "unchanged"
                    if (settingFromUI.SettingValue != "********" && settingFromUI.SettingValue != "[ENCRYPTED]")
                    {
                        // Encrypt the new plaintext value
                        settingToSave.SettingValue = EncryptionService.Encrypt(settingFromUI.SettingValue);
                    }
                    // If it's a placeholder, keep the existing encrypted value (don't update SettingValue)
                }
                else
                {
                    // Non-sensitive or non-encrypted setting
                    settingToSave.SettingValue = settingFromUI.SettingValue;
                }

                // Set timestamps
                if (isUpdate)
                {
                    settingToSave.UpdatedAt = DateTime.UtcNow;
                    _settingsDAL.UpdateSetting(settingToSave);
                }
                else
                {
                    settingToSave.CreatedAt = DateTime.UtcNow;
                    _settingsDAL.InsertSetting(settingToSave);
                }

                // Invalidate cache
                InvalidateCacheEntry(settingFromUI.SettingKey);

                System.Diagnostics.Trace.WriteLine(string.Format("Setting '{0}' {1} successfully", 
                    settingFromUI.SettingKey, isUpdate ? "updated" : "created"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error saving setting '{0}': {1}", 
                    settingFromUI.SettingKey, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets all settings for admin view (encrypted values remain encrypted)
        /// </summary>
        /// <returns>List of all ApplicationSettings</returns>
        public List<ApplicationSetting> GetAllSettingsForAdminView()
        {
            return _settingsDAL.GetAllSettings();
        }

        /// <summary>
        /// Specific getter for Google Client ID
        /// </summary>
        /// <returns>Google Client ID or null</returns>
        public string GetGoogleClientId()
        {
            return GetSettingValue("GoogleClientId");
        }

        /// <summary>
        /// Specific getter for Google Client Secret (encrypted)
        /// </summary>
        /// <returns>Decrypted Google Client Secret or null</returns>
        public string GetGoogleClientSecret()
        {
            return GetDecryptedSettingValue("GoogleClientSecret_Encrypted");
        }

        /// <summary>
        /// Specific getter for OpenAI API Key by setting key name
        /// </summary>
        /// <param name="apiKeySettingKeyNameFromAIModelConfig">Setting key name from AI model configuration</param>
        /// <returns>Decrypted OpenAI API Key or null</returns>
        public string GetOpenAIApiKey(string apiKeySettingKeyNameFromAIModelConfig)
        {
            return GetDecryptedSettingValue(apiKeySettingKeyNameFromAIModelConfig);
        }

        /// <summary>
        /// Specific getter for session timeout in minutes
        /// </summary>
        /// <returns>Session timeout in minutes (default: 30)</returns>
        public int GetSessionTimeoutMinutes()
        {
            return GetIntegerSettingValue("SessionTimeoutMinutes", 30);
        }

        /// <summary>
        /// Invalidates a cache entry for the specified setting key
        /// </summary>
        /// <param name="settingKey">Setting key to invalidate</param>
        private void InvalidateCacheEntry(string settingKey)
        {
            if (string.IsNullOrWhiteSpace(settingKey) || HttpContext.Current?.Cache == null)
                return;

            string cacheKey = CacheKeyPrefix + settingKey;
            HttpContext.Current.Cache.Remove(cacheKey);
        }
    }
}
