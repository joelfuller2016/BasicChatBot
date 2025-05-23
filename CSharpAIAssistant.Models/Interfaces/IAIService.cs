using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Models.Interfaces
{
    /// <summary>
    /// Interface for AI service implementations that process AI tasks
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Processes an AI prompt and returns the response
        /// </summary>
        /// <param name="task">The AI task containing prompt and parameters</param>
        /// <param name="modelConfig">The AI model configuration to use</param>
        /// <param name="apiKey">The API key for the AI service</param>
        /// <returns>AIResponse containing the result and metadata</returns>
        AIResponse ProcessPrompt(AITask task, AIModelConfiguration modelConfig, string apiKey);
    }
}
