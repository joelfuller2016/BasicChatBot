using System;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Contains SQL DDL constants for creating the SQLite database schema
    /// </summary>
    public static class SqlSchemaConstants
    {
        #region Table Creation DDL

        /// <summary>
        /// Creates the Users table for storing user account information
        /// </summary>
        public static readonly string CreateUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                Email TEXT UNIQUE COLLATE NOCASE,
                PasswordHash TEXT,
                GoogleId TEXT UNIQUE,
                RegistrationDate TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
                LastLoginDate TEXT,
                IsAdmin INTEGER NOT NULL DEFAULT 0
            );";

        /// <summary>
        /// Creates the ApplicationSettings table for storing configurable application parameters
        /// </summary>
        public static readonly string CreateApplicationSettingsTable = @"
            CREATE TABLE IF NOT EXISTS ApplicationSettings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SettingKey TEXT NOT NULL UNIQUE COLLATE NOCASE,
                SettingValue TEXT,
                SettingDescription TEXT,
                DataType TEXT NOT NULL,
                IsSensitive INTEGER NOT NULL DEFAULT 0,
                GroupName TEXT,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
                UpdatedAt TEXT
            );";

        /// <summary>
        /// Creates the AIModelConfigurations table for storing AI model configurations
        /// </summary>
        public static readonly string CreateAIModelConfigurationsTable = @"
            CREATE TABLE IF NOT EXISTS AIModelConfigurations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ModelIdentifier TEXT NOT NULL UNIQUE COLLATE NOCASE,
                DisplayName TEXT NOT NULL,
                OpenAISettingKeyForApiKey TEXT NOT NULL,
                DefaultMaxTokens INTEGER,
                DefaultTemperature REAL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
                UpdatedAt TEXT
            );";

        /// <summary>
        /// Creates the AITasks table for storing AI tasks submitted by users
        /// </summary>
        public static readonly string CreateAITasksTable = @"
            CREATE TABLE IF NOT EXISTS AITasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                AIModelConfigurationId INTEGER NOT NULL,
                TaskName TEXT,
                PromptText TEXT NOT NULL,
                Status TEXT NOT NULL,
                MaxTokens INTEGER,
                Temperature REAL,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
                QueuedAt TEXT,
                ProcessingStartedAt TEXT,
                CompletedAt TEXT,
                ErrorMessage TEXT,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                FOREIGN KEY (AIModelConfigurationId) REFERENCES AIModelConfigurations(Id) ON DELETE RESTRICT
            );";

        /// <summary>
        /// Creates the AITaskResults table for storing the results of processed AI tasks
        /// </summary>
        public static readonly string CreateAITaskResultsTable = @"
            CREATE TABLE IF NOT EXISTS AITaskResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AITaskId INTEGER NOT NULL UNIQUE,
                GeneratedContent TEXT,
                TokensUsed_Prompt INTEGER,
                TokensUsed_Completion INTEGER,
                TokensUsed_Total INTEGER,
                ProcessingTimeMs INTEGER,
                ModelUsedIdentifier TEXT,
                Success INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
                FOREIGN KEY (AITaskId) REFERENCES AITasks(Id) ON DELETE CASCADE
            );";

        #endregion

        #region Index Creation DDL

        /// <summary>
        /// Creates an index on Users.Username for faster lookups
        /// </summary>
        public static readonly string CreateUsersUsernameIndex = @"
            CREATE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username COLLATE NOCASE);";

        /// <summary>
        /// Creates an index on Users.Email for faster lookups
        /// </summary>
        public static readonly string CreateUsersEmailIndex = @"
            CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email COLLATE NOCASE);";

        /// <summary>
        /// Creates an index on Users.GoogleId for faster lookups
        /// </summary>
        public static readonly string CreateUsersGoogleIdIndex = @"
            CREATE INDEX IF NOT EXISTS IX_Users_GoogleId ON Users(GoogleId);";

        /// <summary>
        /// Creates an index on ApplicationSettings.SettingKey for faster lookups
        /// </summary>
        public static readonly string CreateApplicationSettingsKeyIndex = @"
            CREATE INDEX IF NOT EXISTS IX_ApplicationSettings_SettingKey ON ApplicationSettings(SettingKey COLLATE NOCASE);";

        /// <summary>
        /// Creates an index on AIModelConfigurations.ModelIdentifier for faster lookups
        /// </summary>
        public static readonly string CreateAIModelConfigurationsIdentifierIndex = @"
            CREATE INDEX IF NOT EXISTS IX_AIModelConfigurations_ModelIdentifier ON AIModelConfigurations(ModelIdentifier COLLATE NOCASE);";

        /// <summary>
        /// Creates an index on AITasks.UserId for faster user task queries
        /// </summary>
        public static readonly string CreateAITasksUserIdIndex = @"
            CREATE INDEX IF NOT EXISTS IX_AITasks_UserId ON AITasks(UserId);";

        /// <summary>
        /// Creates an index on AITasks.Status for faster status-based queries
        /// </summary>
        public static readonly string CreateAITasksStatusIndex = @"
            CREATE INDEX IF NOT EXISTS IX_AITasks_Status ON AITasks(Status);";

        #endregion

        #region Schema Execution Order

        /// <summary>
        /// Array containing all DDL statements in the correct execution order
        /// </summary>
        public static readonly string[] AllDDLStatements = new string[]
        {
            // Tables first
            CreateUsersTable,
            CreateApplicationSettingsTable,
            CreateAIModelConfigurationsTable,
            CreateAITasksTable,
            CreateAITaskResultsTable,
            
            // Indexes after tables
            CreateUsersUsernameIndex,
            CreateUsersEmailIndex,
            CreateUsersGoogleIdIndex,
            CreateApplicationSettingsKeyIndex,
            CreateAIModelConfigurationsIdentifierIndex,
            CreateAITasksUserIdIndex,
            CreateAITasksStatusIndex
        };

        #endregion
    }
}
