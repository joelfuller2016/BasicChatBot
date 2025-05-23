using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Data Access Layer for ApplicationSettings entity
    /// Provides CRUD operations for application configuration settings
    /// </summary>
    public class ApplicationSettingsDAL
    {
        /// <summary>
        /// Retrieves a single ApplicationSetting by its key (case-insensitive)
        /// </summary>
        /// <param name="key">The setting key to search for</param>
        /// <returns>ApplicationSetting if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        public ApplicationSetting GetSettingByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key cannot be null or empty", nameof(key));

            const string sql = @"
                SELECT Id, SettingKey, SettingValue, SettingDescription, DataType, 
                       IsSensitive, GroupName, CreatedAt, UpdatedAt 
                FROM ApplicationSettings 
                WHERE SettingKey = @SettingKey COLLATE NOCASE";

            ApplicationSetting result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToApplicationSetting(reader);
            }, SQLiteHelper.CreateParameter("@SettingKey", key));

            return result;
        }

        /// <summary>
        /// Retrieves all ApplicationSettings ordered by GroupName then SettingKey
        /// </summary>
        /// <returns>List of all ApplicationSettings</returns>
        public List<ApplicationSetting> GetAllSettings()
        {
            const string sql = @"
                SELECT Id, SettingKey, SettingValue, SettingDescription, DataType, 
                       IsSensitive, GroupName, CreatedAt, UpdatedAt 
                FROM ApplicationSettings 
                ORDER BY GroupName, SettingKey";

            var results = new List<ApplicationSetting>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToApplicationSetting(reader));
            });

            return results;
        }

        /// <summary>
        /// Updates an existing ApplicationSetting identified by its SettingKey
        /// </summary>
        /// <param name="setting">ApplicationSetting with updated values</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when setting is null</exception>
        /// <exception cref="ArgumentException">Thrown when SettingKey is null or empty</exception>
        public bool UpdateSetting(ApplicationSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (string.IsNullOrWhiteSpace(setting.SettingKey))
                throw new ArgumentException("SettingKey cannot be null or empty", nameof(setting));

            const string sql = @"
                UPDATE ApplicationSettings 
                SET SettingValue = @SettingValue,
                    SettingDescription = @SettingDescription,
                    DataType = @DataType,
                    IsSensitive = @IsSensitive,
                    GroupName = @GroupName,
                    UpdatedAt = @UpdatedAt
                WHERE SettingKey = @SettingKey COLLATE NOCASE";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@SettingValue", setting.SettingValue),
                SQLiteHelper.CreateParameter("@SettingDescription", setting.SettingDescription),
                SQLiteHelper.CreateParameter("@DataType", setting.DataType ?? "String"),
                SQLiteHelper.CreateParameter("@IsSensitive", setting.IsSensitive ? 1 : 0),
                SQLiteHelper.CreateParameter("@GroupName", setting.GroupName),
                SQLiteHelper.CreateParameter("@UpdatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                SQLiteHelper.CreateParameter("@SettingKey", setting.SettingKey)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Inserts a new ApplicationSetting record
        /// </summary>
        /// <param name="setting">ApplicationSetting to insert</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when setting is null</exception>
        /// <exception cref="ArgumentException">Thrown when SettingKey is null or empty</exception>
        public bool InsertSetting(ApplicationSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (string.IsNullOrWhiteSpace(setting.SettingKey))
                throw new ArgumentException("SettingKey cannot be null or empty", nameof(setting));

            const string sql = @"
                INSERT INTO ApplicationSettings 
                (SettingKey, SettingValue, SettingDescription, DataType, IsSensitive, GroupName, CreatedAt) 
                VALUES (@SettingKey, @SettingValue, @SettingDescription, @DataType, @IsSensitive, @GroupName, @CreatedAt)";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@SettingKey", setting.SettingKey),
                SQLiteHelper.CreateParameter("@SettingValue", setting.SettingValue),
                SQLiteHelper.CreateParameter("@SettingDescription", setting.SettingDescription),
                SQLiteHelper.CreateParameter("@DataType", setting.DataType ?? "String"),
                SQLiteHelper.CreateParameter("@IsSensitive", setting.IsSensitive ? 1 : 0),
                SQLiteHelper.CreateParameter("@GroupName", setting.GroupName),
                SQLiteHelper.CreateParameter("@CreatedAt", setting.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Maps data from an SQLiteDataReader row to an ApplicationSetting POCO
        /// </summary>
        /// <param name="reader">SQLiteDataReader positioned at a valid row</param>
        /// <returns>Populated ApplicationSetting object</returns>
        private ApplicationSetting MapReaderToApplicationSetting(SQLiteDataReader reader)
        {
            return new ApplicationSetting
            {
                Id = Convert.ToInt32(reader["Id"]),
                SettingKey = reader["SettingKey"]?.ToString(),
                SettingValue = reader["SettingValue"]?.ToString(),
                SettingDescription = reader["SettingDescription"]?.ToString(),
                DataType = reader["DataType"]?.ToString() ?? "String",
                IsSensitive = Convert.ToBoolean(reader["IsSensitive"]),
                GroupName = reader["GroupName"]?.ToString(),
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? 
                    DateTime.Parse(reader["UpdatedAt"].ToString()) : (DateTime?)null
            };
        }
    }
}
