using System;

namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents an AI task submitted by a user
    /// </summary>
    public class AITask
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this task
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the AI model configuration to use for this task
        /// </summary>
        public int AIModelConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the optional name/title for this task
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets the prompt text to send to the AI model
        /// </summary>
        public string PromptText { get; set; }

        /// <summary>
        /// Gets or sets the current status of the task
        /// Expected values: "Pending", "Queued", "Processing", "Completed", "Failed"
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens for this specific task (overrides model default)
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature setting for this specific task (overrides model default)
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this task was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this task was queued for processing
        /// </summary>
        public DateTime? QueuedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when processing of this task began
        /// </summary>
        public DateTime? ProcessingStartedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this task was completed (successfully or with failure)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the error message if the task failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the AITask class
        /// </summary>
        public AITask()
        {
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
        }
    }
}
