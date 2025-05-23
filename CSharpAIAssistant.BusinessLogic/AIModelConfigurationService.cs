using System;
using System.Collections.Generic;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Business logic service for managing AI model configurations
    /// </summary>
    public class AIModelConfigurationService
    {
        private readonly AIModelConfigurationDAL _modelConfigDAL;

        /// <summary>
        /// Initializes a new instance with default AIModelConfigurationDAL
        /// </summary>
        public AIModelConfigurationService()
        {
            _modelConfigDAL = new AIModelConfigurationDAL();
        }

        /// <summary>
        /// Initializes a new instance with provided AIModelConfigurationDAL for dependency injection
        /// </summary>
        /// <param name="modelConfigDAL">AIModelConfigurationDAL instance</param>
        public AIModelConfigurationService(AIModelConfigurationDAL modelConfigDAL)
        {
            _modelConfigDAL = modelConfigDAL ?? throw new ArgumentNullException(nameof(modelConfigDAL));
        }

        /// <summary>
        /// Gets active AI model configurations available for user selection
        /// </summary>
        /// <returns>List of active AI model configurations</returns>
        public List<AIModelConfiguration> GetActiveModelsForUserSelection()
        {
            try
            {
                return _modelConfigDAL.GetActiveModels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving active AI models: {0}", ex.ToString());
                throw new InvalidOperationException("Failed to retrieve available AI models", ex);
            }
        }

        /// <summary>
        /// Gets an AI model configuration by its ID
        /// </summary>
        /// <param name="id">The ID of the model configuration</param>
        /// <returns>AIModelConfiguration if found, null otherwise</returns>
        public AIModelConfiguration GetModelById(int id)
        {
            try
            {
                if (id <= 0)
                    return null;

                return _modelConfigDAL.GetById(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving AI model by ID {0}: {1}", id, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets an AI model configuration by its model identifier
        /// </summary>
        /// <param name="modelIdentifier">The model identifier (e.g., "gpt-3.5-turbo")</param>
        /// <returns>AIModelConfiguration if found, null otherwise</returns>
        public AIModelConfiguration GetModelByIdentifier(string modelIdentifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelIdentifier))
                    return null;

                return _modelConfigDAL.GetByModelIdentifier(modelIdentifier);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving AI model by identifier '{0}': {1}", modelIdentifier, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets all AI model configurations for admin management
        /// </summary>
        /// <returns>List of all AI model configurations</returns>
        public List<AIModelConfiguration> GetAllModelsForAdmin()
        {
            try
            {
                return _modelConfigDAL.GetAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error retrieving all AI models: {0}", ex.ToString());
                throw new InvalidOperationException("Failed to retrieve AI model configurations", ex);
            }
        }

        /// <summary>
        /// Saves an AI model configuration (insert or update based on ID)
        /// </summary>
        /// <param name="config">AIModelConfiguration to save</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public bool SaveModelConfiguration(AIModelConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                // Validate required fields
                ValidateModelConfiguration(config);

                bool isUpdate = config.Id > 0;
                bool success;

                if (isUpdate)
                {
                    config.UpdatedAt = DateTime.UtcNow;
                    success = _modelConfigDAL.Update(config);
                }
                else
                {
                    config.CreatedAt = DateTime.UtcNow;
                    success = _modelConfigDAL.Insert(config);
                }

                if (success)
                {
                    System.Diagnostics.Trace.WriteLine("AI model configuration {0}: {1} ({2})", 
                        isUpdate ? "updated" : "created", config.DisplayName, config.ModelIdentifier);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error saving AI model configuration: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Deletes an AI model configuration
        /// Note: This may fail if there are associated AI tasks
        /// </summary>
        /// <param name="id">ID of the configuration to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool DeleteModelConfiguration(int id)
        {
            try
            {
                if (id <= 0)
                    return false;

                var config = _modelConfigDAL.GetById(id);
                if (config == null)
                    return false;

                bool success = _modelConfigDAL.Delete(id);
                
                if (success)
                {
                    System.Diagnostics.Trace.WriteLine("AI model configuration deleted: {0} ({1})", 
                        config.DisplayName, config.ModelIdentifier);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error deleting AI model configuration {0}: {1}", id, ex.ToString());
                
                // Check if this is a foreign key constraint error
                if (ex.Message.Contains("FOREIGN KEY") || ex.Message.Contains("constraint"))
                {
                    throw new InvalidOperationException("Cannot delete model configuration because it is being used by existing AI tasks. Please deactivate it instead or delete the associated tasks first.", ex);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Activates or deactivates an AI model configuration
        /// </summary>
        /// <param name="id">ID of the configuration</param>
        /// <param name="isActive">True to activate, false to deactivate</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetModelActiveStatus(int id, bool isActive)
        {
            try
            {
                var config = _modelConfigDAL.GetById(id);
                if (config == null)
                    return false;

                config.IsActive = isActive;
                config.UpdatedAt = DateTime.UtcNow;

                bool success = _modelConfigDAL.Update(config);
                
                if (success)
                {
                    System.Diagnostics.Trace.WriteLine("AI model configuration {0}: {1} ({2})", 
                        isActive ? "activated" : "deactivated", config.DisplayName, config.ModelIdentifier);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error setting AI model active status for ID {0}: {1}", id, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Validates that an AI model configuration has all required fields
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        private void ValidateModelConfiguration(AIModelConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.ModelIdentifier))
                throw new ArgumentException("Model identifier is required");

            if (string.IsNullOrWhiteSpace(config.DisplayName))
                throw new ArgumentException("Display name is required");

            if (string.IsNullOrWhiteSpace(config.OpenAISettingKeyForApiKey))
                throw new ArgumentException("API key setting reference is required");

            // Validate numeric ranges
            if (config.DefaultMaxTokens.HasValue && config.DefaultMaxTokens.Value <= 0)
                throw new ArgumentException("Default max tokens must be greater than 0");

            if (config.DefaultMaxTokens.HasValue && config.DefaultMaxTokens.Value > 100000)
                throw new ArgumentException("Default max tokens cannot exceed 100,000");

            if (config.DefaultTemperature.HasValue && (config.DefaultTemperature.Value < 0 || config.DefaultTemperature.Value > 2))
                throw new ArgumentException("Default temperature must be between 0 and 2");

            // Check for duplicate model identifier (would need to be enhanced for update scenarios)
            if (config.Id == 0) // Only check for new configurations
            {
                var existing = _modelConfigDAL.GetByModelIdentifier(config.ModelIdentifier);
                if (existing != null)
                    throw new ArgumentException($"A model configuration with identifier '{config.ModelIdentifier}' already exists");
            }
        }
    }
}
