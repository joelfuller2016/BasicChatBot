using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Data Access Layer for AIModelConfiguration entity
    /// Provides CRUD operations for AI model configurations
    /// </summary>
    public class AIModelConfigurationDAL
    {
        /// <summary>
        /// Retrieves all AI model configurations
        /// </summary>
        /// <returns>List of all AIModelConfiguration records</returns>
        public List<AIModelConfiguration> GetAll()
        {
            const string sql = @"
                SELECT Id, ModelIdentifier, DisplayName, OpenAISettingKeyForApiKey,
                       DefaultMaxTokens, DefaultTemperature, IsActive, Notes,
                       CreatedAt, UpdatedAt
                FROM AIModelConfigurations
                ORDER BY DisplayName";

            var results = new List<AIModelConfiguration>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToAIModelConfiguration(reader));
            });

            return results;
        }

        /// <summary>
        /// Retrieves only active AI model configurations
        /// </summary>
        /// <returns>List of active AIModelConfiguration records</returns>
        public List<AIModelConfiguration> GetActiveModels()
        {
            const string sql = @"
                SELECT Id, ModelIdentifier, DisplayName, OpenAISettingKeyForApiKey,
                       DefaultMaxTokens, DefaultTemperature, IsActive, Notes,
                       CreatedAt, UpdatedAt
                FROM AIModelConfigurations
                WHERE IsActive = 1
                ORDER BY DisplayName";

            var results = new List<AIModelConfiguration>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToAIModelConfiguration(reader));
            });

            return results;
        }

        /// <summary>
        /// Retrieves an AI model configuration by its primary key
        /// </summary>
        /// <param name="id">The ID of the model configuration</param>
        /// <returns>AIModelConfiguration if found, null otherwise</returns>
        public AIModelConfiguration GetById(int id)
        {
            const string sql = @"
                SELECT Id, ModelIdentifier, DisplayName, OpenAISettingKeyForApiKey,
                       DefaultMaxTokens, DefaultTemperature, IsActive, Notes,
                       CreatedAt, UpdatedAt
                FROM AIModelConfigurations
                WHERE Id = @Id";

            AIModelConfiguration result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToAIModelConfiguration(reader);
            }, SQLiteHelper.CreateParameter("@Id", id));

            return result;
        }

        /// <summary>
        /// Retrieves an AI model configuration by its model identifier
        /// </summary>
        /// <param name="modelIdentifier">The model identifier (e.g., "gpt-3.5-turbo")</param>
        /// <returns>AIModelConfiguration if found, null otherwise</returns>
        public AIModelConfiguration GetByModelIdentifier(string modelIdentifier)
        {
            if (string.IsNullOrWhiteSpace(modelIdentifier))
                throw new ArgumentException("Model identifier cannot be null or empty", nameof(modelIdentifier));

            const string sql = @"
                SELECT Id, ModelIdentifier, DisplayName, OpenAISettingKeyForApiKey,
                       DefaultMaxTokens, DefaultTemperature, IsActive, Notes,
                       CreatedAt, UpdatedAt
                FROM AIModelConfigurations
                WHERE ModelIdentifier = @ModelIdentifier COLLATE NOCASE";

            AIModelConfiguration result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToAIModelConfiguration(reader);
            }, SQLiteHelper.CreateParameter("@ModelIdentifier", modelIdentifier));

            return result;
        }

        /// <summary>
        /// Inserts a new AI model configuration
        /// </summary>
        /// <param name="config">AIModelConfiguration to insert</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public bool Insert(AIModelConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.ModelIdentifier))
                throw new ArgumentException("ModelIdentifier is required", nameof(config));

            if (string.IsNullOrWhiteSpace(config.DisplayName))
                throw new ArgumentException("DisplayName is required", nameof(config));

            if (string.IsNullOrWhiteSpace(config.OpenAISettingKeyForApiKey))
                throw new ArgumentException("OpenAISettingKeyForApiKey is required", nameof(config));

            const string sql = @"
                INSERT INTO AIModelConfigurations 
                (ModelIdentifier, DisplayName, OpenAISettingKeyForApiKey, DefaultMaxTokens, 
                 DefaultTemperature, IsActive, Notes, CreatedAt) 
                VALUES (@ModelIdentifier, @DisplayName, @OpenAISettingKeyForApiKey, @DefaultMaxTokens,
                        @DefaultTemperature, @IsActive, @Notes, @CreatedAt)";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@ModelIdentifier", config.ModelIdentifier),
                SQLiteHelper.CreateParameter("@DisplayName", config.DisplayName),
                SQLiteHelper.CreateParameter("@OpenAISettingKeyForApiKey", config.OpenAISettingKeyForApiKey),
                SQLiteHelper.CreateParameter("@DefaultMaxTokens", config.DefaultMaxTokens),
                SQLiteHelper.CreateParameter("@DefaultTemperature", config.DefaultTemperature),
                SQLiteHelper.CreateParameter("@IsActive", config.IsActive ? 1 : 0),
                SQLiteHelper.CreateParameter("@Notes", config.Notes),
                SQLiteHelper.CreateParameter("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Updates an existing AI model configuration
        /// </summary>
        /// <param name="config">AIModelConfiguration with updated values</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public bool Update(AIModelConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.Id <= 0)
                throw new ArgumentException("Valid ID is required for update", nameof(config));

            if (string.IsNullOrWhiteSpace(config.ModelIdentifier))
                throw new ArgumentException("ModelIdentifier is required", nameof(config));

            if (string.IsNullOrWhiteSpace(config.DisplayName))
                throw new ArgumentException("DisplayName is required", nameof(config));

            if (string.IsNullOrWhiteSpace(config.OpenAISettingKeyForApiKey))
                throw new ArgumentException("OpenAISettingKeyForApiKey is required", nameof(config));

            const string sql = @"
                UPDATE AIModelConfigurations 
                SET ModelIdentifier = @ModelIdentifier,
                    DisplayName = @DisplayName,
                    OpenAISettingKeyForApiKey = @OpenAISettingKeyForApiKey,
                    DefaultMaxTokens = @DefaultMaxTokens,
                    DefaultTemperature = @DefaultTemperature,
                    IsActive = @IsActive,
                    Notes = @Notes,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@ModelIdentifier", config.ModelIdentifier),
                SQLiteHelper.CreateParameter("@DisplayName", config.DisplayName),
                SQLiteHelper.CreateParameter("@OpenAISettingKeyForApiKey", config.OpenAISettingKeyForApiKey),
                SQLiteHelper.CreateParameter("@DefaultMaxTokens", config.DefaultMaxTokens),
                SQLiteHelper.CreateParameter("@DefaultTemperature", config.DefaultTemperature),
                SQLiteHelper.CreateParameter("@IsActive", config.IsActive ? 1 : 0),
                SQLiteHelper.CreateParameter("@Notes", config.Notes),
                SQLiteHelper.CreateParameter("@UpdatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                SQLiteHelper.CreateParameter("@Id", config.Id)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Deletes an AI model configuration by ID
        /// Note: This may fail if there are associated AI tasks due to foreign key constraints
        /// </summary>
        /// <param name="id">ID of the configuration to delete</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool Delete(int id)
        {
            const string sql = "DELETE FROM AIModelConfigurations WHERE Id = @Id";

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, 
                SQLiteHelper.CreateParameter("@Id", id));
            
            return rowsAffected == 1;
        }

        /// <summary>
        /// Maps data from an SQLiteDataReader row to an AIModelConfiguration POCO
        /// </summary>
        /// <param name="reader">SQLiteDataReader positioned at a valid row</param>
        /// <returns>Populated AIModelConfiguration object</returns>
        private AIModelConfiguration MapReaderToAIModelConfiguration(SQLiteDataReader reader)
        {
            return new AIModelConfiguration
            {
                Id = Convert.ToInt32(reader["Id"]),
                ModelIdentifier = reader["ModelIdentifier"]?.ToString(),
                DisplayName = reader["DisplayName"]?.ToString(),
                OpenAISettingKeyForApiKey = reader["OpenAISettingKeyForApiKey"]?.ToString(),
                DefaultMaxTokens = reader["DefaultMaxTokens"] != DBNull.Value ? 
                    Convert.ToInt32(reader["DefaultMaxTokens"]) : (int?)null,
                DefaultTemperature = reader["DefaultTemperature"] != DBNull.Value ? 
                    Convert.ToDouble(reader["DefaultTemperature"]) : (double?)null,
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                Notes = reader["Notes"]?.ToString(),
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? 
                    DateTime.Parse(reader["UpdatedAt"].ToString()) : (DateTime?)null
            };
        }
    }
}
