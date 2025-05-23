using System;
using System.Collections.Generic;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Business logic service for managing AI tasks
    /// </summary>
    public class AITaskService
    {
        private readonly AITaskDAL _aiTaskDAL;
        private readonly AITaskResultDAL _aiTaskResultDAL;
        private readonly ApplicationSettingsService _applicationSettingsService;
        private readonly AIModelConfigurationService _aiModelConfigurationService;

        /// <summary>
        /// Initializes a new instance with default dependencies
        /// </summary>
        public AITaskService()
        {
            _aiTaskDAL = new AITaskDAL();
            _aiTaskResultDAL = new AITaskResultDAL();
            _applicationSettingsService = new ApplicationSettingsService();
            _aiModelConfigurationService = new AIModelConfigurationService();
        }

        /// <summary>
        /// Initializes a new instance with provided dependencies for dependency injection
        /// </summary>
        public AITaskService(AITaskDAL aiTaskDAL, AITaskResultDAL aiTaskResultDAL, 
            ApplicationSettingsService applicationSettingsService, 
            AIModelConfigurationService aiModelConfigurationService)
        {
            _aiTaskDAL = aiTaskDAL ?? throw new ArgumentNullException(nameof(aiTaskDAL));
            _aiTaskResultDAL = aiTaskResultDAL ?? throw new ArgumentNullException(nameof(aiTaskResultDAL));
            _applicationSettingsService = applicationSettingsService ?? throw new ArgumentNullException(nameof(applicationSettingsService));
            _aiModelConfigurationService = aiModelConfigurationService ?? throw new ArgumentNullException(nameof(aiModelConfigurationService));
        }

        /// <summary>
        /// Creates a new AI task and queues it for processing
        /// </summary>
        /// <param name="userId">ID of the user creating the task</param>
        /// <param name="aiModelConfigurationId">ID of the AI model configuration to use</param>
        /// <param name="taskName">Optional name for the task</param>
        /// <param name="promptText">The prompt text to send to the AI</param>
        /// <param name="maxTokensOverride">Optional override for max tokens</param>
        /// <param name="temperatureOverride">Optional override for temperature</param>
        /// <returns>ID of the newly created task, or 0 if creation failed</returns>
        public int CreateNewTask(int userId, int aiModelConfigurationId, string taskName, 
            string promptText, int? maxTokensOverride = null, double? temperatureOverride = null)
        {
            try
            {
                // Validate inputs
                if (userId <= 0)
                    throw new ArgumentException("Valid user ID is required", nameof(userId));

                if (aiModelConfigurationId <= 0)
                    throw new ArgumentException("Valid AI model configuration ID is required", nameof(aiModelConfigurationId));

                if (string.IsNullOrWhiteSpace(promptText))
                    throw new ArgumentException("Prompt text is required", nameof(promptText));

                // Get the AI model configuration
                var modelConfig = _aiModelConfigurationService.GetModelById(aiModelConfigurationId);
                if (modelConfig == null)
                    throw new InvalidOperationException($"AI model configuration with ID {aiModelConfigurationId} not found");

                if (!modelConfig.IsActive)
                    throw new InvalidOperationException("The selected AI model is not currently active");

                // Determine max tokens
                int maxTokens = maxTokensOverride ?? 
                               modelConfig.DefaultMaxTokens ?? 
                               _applicationSettingsService.GetIntegerSettingValue("DefaultAITaskMaxTokens", 1000);

                // Validate max tokens range
                if (maxTokens <= 0 || maxTokens > 100000)
                    throw new ArgumentException("Max tokens must be between 1 and 100,000");

                // Determine temperature
                double temperature = temperatureOverride ?? 
                                   modelConfig.DefaultTemperature ?? 
                                   _applicationSettingsService.GetRealSettingValue("DefaultAITaskTemperature", 0.7);

                // Validate temperature range
                if (temperature < 0 || temperature > 2)
                    throw new ArgumentException("Temperature must be between 0 and 2");

                // Create the task
                var task = new AITask
                {
                    UserId = userId,
                    AIModelConfigurationId = aiModelConfigurationId,
                    TaskName = string.IsNullOrWhiteSpace(taskName) ? null : taskName.Trim(),
                    PromptText = promptText.Trim(),
                    Status = "Pending",
                    MaxTokens = maxTokens,
                    Temperature = temperature,
                    CreatedAt = DateTime.UtcNow
                };

                // Insert the task
                int newTaskId = _aiTaskDAL.Insert(task);

                if (newTaskId > 0)
                {
                    // Update status to Queued and queue for processing
                    bool statusUpdated = _aiTaskDAL.UpdateStatus(newTaskId, "Queued", DateTime.UtcNow);
                    
                    if (statusUpdated)
                    {
                        // Queue the task for background processing
                        AITaskProcessor.QueueTask(newTaskId);
                        
                        System.Diagnostics.Trace.WriteLine(
                            $"AI task created and queued: ID={newTaskId}, User={userId}, Model={modelConfig.ModelIdentifier}, MaxTokens={maxTokens}, Temperature={temperature}");
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceWarning("Task created but failed to update status to Queued: TaskID={0}", newTaskId);
                    }
                }

                return newTaskId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error creating AI task: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets AI tasks for a specific user with pagination
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of tasks per page</param>
        /// <returns>List of AI tasks for the user</returns>
        public List<AITask> GetUserTasks(int userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Valid user ID is required", nameof(userId));

                return _aiTaskDAL.GetByUserId(userId, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving user tasks for user {0}: {1}", userId, ex.ToString());
                throw new InvalidOperationException("Failed to retrieve user tasks", ex);
            }
        }

        /// <summary>
        /// Gets detailed information for a specific task including its result
        /// </summary>
        /// <param name="taskId">ID of the task</param>
        /// <param name="userId">ID of the user requesting the task (for authorization)</param>
        /// <param name="isAdmin">Whether the requesting user is an admin</param>
        /// <returns>Tuple containing the task and its result, or null if not found/unauthorized</returns>
        public Tuple<AITask, AITaskResult> GetTaskDetailsWithResult(int taskId, int userId, bool isAdmin = false)
        {
            try
            {
                if (taskId <= 0)
                    return null;

                // Get the task
                var task = _aiTaskDAL.GetById(taskId);
                if (task == null)
                    return null;

                // Check authorization - user can only view their own tasks unless they're admin
                if (!isAdmin && task.UserId != userId)
                {
                    System.Diagnostics.Trace.TraceWarning("Unauthorized task access attempt: TaskID={0}, RequestingUser={1}, TaskOwner={2}", 
                        taskId, userId, task.UserId);
                    return null;
                }

                // Get the result
                var result = _aiTaskResultDAL.GetByAITaskId(taskId);

                return Tuple.Create(task, result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving task details for task {0}: {1}", taskId, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the count of tasks by status for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>Count of matching tasks</returns>
        public int GetUserTaskCount(int userId, string status = null)
        {
            try
            {
                if (userId <= 0)
                    return 0;

                return _aiTaskDAL.GetTaskCountByStatus(userId, status);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error getting task count for user {0}: {1}", userId, ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Gets statistics for a user's AI task usage
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Tuple containing (TotalTasks, SuccessfulTasks, TotalTokensUsed)</returns>
        public (int TotalTasks, int SuccessfulTasks, long TotalTokensUsed) GetUserStatistics(int userId)
        {
            try
            {
                if (userId <= 0)
                    return (0, 0, 0);

                return _aiTaskResultDAL.GetUserStatistics(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error getting user statistics for user {0}: {1}", userId, ex.ToString());
                return (0, 0, 0);
            }
        }

        /// <summary>
        /// Gets tasks that are queued and ready for processing
        /// </summary>
        /// <param name="limit">Maximum number of tasks to retrieve</param>
        /// <returns>List of queued tasks</returns>
        internal List<AITask> GetQueuedTasks(int limit = 10)
        {
            try
            {
                return _aiTaskDAL.GetQueuedTasks(limit);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving queued tasks: {0}", ex.ToString());
                return new List<AITask>();
            }
        }

        /// <summary>
        /// Updates the status of a task (internal method for AITaskProcessor)
        /// </summary>
        /// <param name="taskId">ID of the task</param>
        /// <param name="newStatus">New status</param>
        /// <param name="errorMessage">Optional error message</param>
        /// <returns>True if successful</returns>
        internal bool UpdateTaskStatus(int taskId, string newStatus, string errorMessage = null)
        {
            try
            {
                return _aiTaskDAL.UpdateStatus(taskId, newStatus, DateTime.UtcNow, errorMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error updating task status for task {0}: {1}", taskId, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Saves a task result (internal method for AITaskProcessor)
        /// </summary>
        /// <param name="result">Task result to save</param>
        /// <returns>True if successful</returns>
        internal bool SaveTaskResult(AITaskResult result)
        {
            try
            {
                return _aiTaskResultDAL.Insert(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error saving task result for task {0}: {1}", result?.AITaskId, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Validates task creation parameters
        /// </summary>
        /// <param name="promptText">Prompt text to validate</param>
        /// <param name="maxTokens">Max tokens to validate</param>
        /// <param name="temperature">Temperature to validate</param>
        /// <returns>Validation error message, or null if valid</returns>
        public string ValidateTaskParameters(string promptText, int? maxTokens = null, double? temperature = null)
        {
            if (string.IsNullOrWhiteSpace(promptText))
                return "Prompt text is required";

            if (promptText.Length > 10000)
                return "Prompt text cannot exceed 10,000 characters";

            if (maxTokens.HasValue && (maxTokens.Value <= 0 || maxTokens.Value > 100000))
                return "Max tokens must be between 1 and 100,000";

            if (temperature.HasValue && (temperature.Value < 0 || temperature.Value > 2))
                return "Temperature must be between 0 and 2";

            return null; // Valid
        }
    }
}
