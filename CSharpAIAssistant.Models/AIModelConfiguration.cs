using System;

namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents a configuration for an AI model that can be used in the system
    /// </summary>
    public class AIModelConfiguration
    {
        /// <summary>
        /// Gets or sets the unique identifier for the model configuration
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the AI model (e.g., "gpt-3.5-turbo")
        /// </summary>
        public string ModelIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly display name for the model
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the key in ApplicationSettings that contains the encrypted API key for this model
        /// </summary>
        public string OpenAISettingKeyForApiKey { get; set; }

        /// <summary>
        /// Gets or sets the default maximum number of tokens for requests using this model
        /// </summary>
        public int? DefaultMaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the default temperature (randomness) setting for requests using this model
        /// </summary>
        public double? DefaultTemperature { get; set; }

        /// <summary>
        /// Gets or sets whether this model configuration is active and available for use
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets additional notes or comments about this model configuration
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this configuration was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this configuration was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the AIModelConfiguration class
        /// </summary>
        public AIModelConfiguration()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            DefaultTemperature = 0.7;
            DefaultMaxTokens = 1000;
        }
    }
}
