using System;

namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents an application configuration setting
    /// </summary>
    public class ApplicationSetting
    {
        /// <summary>
        /// Gets or sets the unique identifier for the setting
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique key for the setting (case-insensitive)
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Gets or sets the value of the setting (may be encrypted if sensitive)
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Gets or sets the description of what this setting controls
        /// </summary>
        public string SettingDescription { get; set; }

        /// <summary>
        /// Gets or sets the data type of the setting value
        /// Expected values: "String", "EncryptedString", "Boolean", "Integer", "Real"
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets whether this setting contains sensitive data that should be encrypted
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        /// Gets or sets the logical grouping for this setting (for UI organization)
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this setting was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this setting was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the ApplicationSetting class
        /// </summary>
        public ApplicationSetting()
        {
            CreatedAt = DateTime.UtcNow;
            DataType = "String";
            IsSensitive = false;
        }
    }
}
