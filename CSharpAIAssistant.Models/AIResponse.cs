namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents the response from an AI service after processing a task
    /// </summary>
    public class AIResponse
    {
        /// <summary>
        /// Gets or sets whether the AI processing was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the generated text response from the AI
        /// </summary>
        public string GeneratedText { get; set; }

        /// <summary>
        /// Gets or sets the error message if processing failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens used in the prompt
        /// </summary>
        public int? TokensUsedPrompt { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens used in the completion/response
        /// </summary>
        public int? TokensUsedCompletion { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens used for this request
        /// </summary>
        public int? TokensUsedTotal { get; set; }

        /// <summary>
        /// Gets or sets the processing time in milliseconds
        /// </summary>
        public long? ProcessingTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the model that was actually used
        /// </summary>
        public string ModelUsed { get; set; }

        /// <summary>
        /// Initializes a new instance of the AIResponse class
        /// </summary>
        public AIResponse()
        {
            Success = false;
        }

        /// <summary>
        /// Creates a successful AI response
        /// </summary>
        /// <param name="generatedText">The generated text</param>
        /// <param name="modelUsed">The model identifier</param>
        /// <param name="processingTimeMs">Processing time</param>
        /// <param name="tokensPrompt">Prompt tokens</param>
        /// <param name="tokensCompletion">Completion tokens</param>
        /// <param name="tokensTotal">Total tokens</param>
        /// <returns>Configured AIResponse instance</returns>
        public static AIResponse CreateSuccess(string generatedText, string modelUsed, 
            long? processingTimeMs = null, int? tokensPrompt = null, 
            int? tokensCompletion = null, int? tokensTotal = null)
        {
            return new AIResponse
            {
                Success = true,
                GeneratedText = generatedText,
                ModelUsed = modelUsed,
                ProcessingTimeMs = processingTimeMs,
                TokensUsedPrompt = tokensPrompt,
                TokensUsedCompletion = tokensCompletion,
                TokensUsedTotal = tokensTotal
            };
        }

        /// <summary>
        /// Creates a failed AI response
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="modelUsed">The model identifier (if known)</param>
        /// <param name="processingTimeMs">Processing time (if available)</param>
        /// <returns>Configured AIResponse instance</returns>
        public static AIResponse CreateFailure(string errorMessage, string modelUsed = null, long? processingTimeMs = null)
        {
            return new AIResponse
            {
                Success = false,
                ErrorMessage = errorMessage,
                ModelUsed = modelUsed,
                ProcessingTimeMs = processingTimeMs
            };
        }
    }
}
