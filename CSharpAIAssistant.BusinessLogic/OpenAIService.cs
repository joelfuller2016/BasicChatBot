using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpAIAssistant.Models;
using CSharpAIAssistant.Models.Interfaces;
using Newtonsoft.Json;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Service for integrating with OpenAI's Chat Completions API
    /// Implements IAIService interface for processing AI prompts
    /// </summary>
    public class OpenAIService : IAIService
    {
        #region Private Fields

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string OPENAI_API_ENDPOINT = "https://api.openai.com/v1/chat/completions";
        private const int DEFAULT_MAX_TOKENS = 1000;
        private const double DEFAULT_TEMPERATURE = 0.7;
        private const int MAX_ALLOWED_TOKENS = 4000;

        #endregion

        #region Constructor

        static OpenAIService()
        {
            // Configure HTTP client timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes an AI prompt using OpenAI's Chat Completions API
        /// </summary>
        /// <param name="task">The AI task containing the prompt and parameters</param>
        /// <param name="modelConfig">Configuration for the AI model to use</param>
        /// <param name="apiKey">OpenAI API key for authentication</param>
        /// <returns>AIResponse containing the generated content and metadata</returns>
        public AIResponse ProcessPrompt(AITask task, AIModelConfiguration modelConfig, string apiKey)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AIResponse response = new AIResponse
            {
                ModelUsed = modelConfig.ModelIdentifier
            };

            try
            {
                Trace.WriteLine($"OpenAIService: Processing prompt for task {task.Id} using model {modelConfig.ModelIdentifier}");

                // Validate inputs
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    response.Success = false;
                    response.ErrorMessage = "API key is required but not provided";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(task.PromptText))
                {
                    response.Success = false;
                    response.ErrorMessage = "Prompt text is required but not provided";
                    return response;
                }

                // Determine parameters
                int maxTokens = DetermineMaxTokens(task, modelConfig);
                double temperature = DetermineTemperature(task, modelConfig);

                // Create the request payload
                var requestPayload = new
                {
                    model = modelConfig.ModelIdentifier,
                    messages = new[]
                    {
                        new { role = "user", content = task.PromptText }
                    },
                    max_tokens = maxTokens,
                    temperature = temperature,
                    stream = false
                };

                // Serialize request to JSON
                string jsonPayload = JsonConvert.SerializeObject(requestPayload, Formatting.None);
                Trace.WriteLine($"OpenAIService: Request payload size: {jsonPayload.Length} characters");

                // Make the API call
                var apiResponse = MakeOpenAIApiCall(jsonPayload, apiKey);
                stopwatch.Stop();

                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

                if (apiResponse.IsSuccess)
                {
                    // Parse successful response
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIApiResponse>(apiResponse.ResponseBody);
                    
                    response.Success = true;
                    response.GeneratedText = ExtractGeneratedText(openAIResponse);
                    response.TokensUsedPrompt = openAIResponse?.usage?.prompt_tokens;
                    response.TokensUsedCompletion = openAIResponse?.usage?.completion_tokens;
                    response.TokensUsedTotal = openAIResponse?.usage?.total_tokens;

                    Trace.WriteLine($"OpenAIService: Successfully processed task {task.Id}. Tokens used: {response.TokensUsedTotal}");
                }
                else
                {
                    // Parse error response
                    response.Success = false;
                    response.ErrorMessage = ParseErrorMessage(apiResponse.StatusCode, apiResponse.ResponseBody);
                    
                    Trace.TraceError($"OpenAIService: API error for task {task.Id}: {response.ErrorMessage}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                stopwatch.Stop();
                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                response.Success = false;
                response.ErrorMessage = $"Network error while calling OpenAI API: {httpEx.Message}";
                Trace.TraceError($"OpenAIService: HTTP error for task {task.Id}: {httpEx}");
            }
            catch (TaskCanceledException timeoutEx)
            {
                stopwatch.Stop();
                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                response.Success = false;
                response.ErrorMessage = "Request timed out while waiting for OpenAI API response";
                Trace.TraceError($"OpenAIService: Timeout error for task {task.Id}: {timeoutEx}");
            }
            catch (JsonException jsonEx)
            {
                stopwatch.Stop();
                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                response.Success = false;
                response.ErrorMessage = $"Error parsing OpenAI API response: {jsonEx.Message}";
                Trace.TraceError($"OpenAIService: JSON parsing error for task {task.Id}: {jsonEx}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                response.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                response.Success = false;
                response.ErrorMessage = $"Unexpected error during OpenAI API call: {ex.Message}";
                Trace.TraceError($"OpenAIService: Unexpected error for task {task.Id}: {ex}");
            }

            return response;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes the actual HTTP call to the OpenAI API
        /// </summary>
        private ApiCallResult MakeOpenAIApiCall(string jsonPayload, string apiKey)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_ENDPOINT))
                {
                    // Set headers
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    request.Headers.Add("User-Agent", "CSharpAIAssistant/1.0");

                    // Set content
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Execute request synchronously (since we're in a background thread)
                    using (var httpResponse = _httpClient.SendAsync(request).Result)
                    {
                        string responseBody = httpResponse.Content.ReadAsStringAsync().Result;
                        
                        return new ApiCallResult
                        {
                            IsSuccess = httpResponse.IsSuccessStatusCode,
                            StatusCode = (int)httpResponse.StatusCode,
                            ResponseBody = responseBody
                        };
                    }
                }
            }
            catch (AggregateException aggEx)
            {
                // Unwrap aggregate exceptions from async operations
                var innerEx = aggEx.InnerException ?? aggEx;
                throw innerEx;
            }
        }

        /// <summary>
        /// Determines the maximum tokens to use for the request
        /// </summary>
        private int DetermineMaxTokens(AITask task, AIModelConfiguration modelConfig)
        {
            // Priority: Task override > Model default > Application default
            if (task.MaxTokens.HasValue && task.MaxTokens.Value > 0)
            {
                return Math.Min(task.MaxTokens.Value, MAX_ALLOWED_TOKENS);
            }
            
            if (modelConfig.DefaultMaxTokens.HasValue && modelConfig.DefaultMaxTokens.Value > 0)
            {
                return Math.Min(modelConfig.DefaultMaxTokens.Value, MAX_ALLOWED_TOKENS);
            }
            
            return DEFAULT_MAX_TOKENS;
        }

        /// <summary>
        /// Determines the temperature to use for the request
        /// </summary>
        private double DetermineTemperature(AITask task, AIModelConfiguration modelConfig)
        {
            // Priority: Task override > Model default > Application default
            if (task.Temperature.HasValue)
            {
                return Math.Max(0.0, Math.Min(2.0, task.Temperature.Value));
            }
            
            if (modelConfig.DefaultTemperature.HasValue)
            {
                return Math.Max(0.0, Math.Min(2.0, modelConfig.DefaultTemperature.Value));
            }
            
            return DEFAULT_TEMPERATURE;
        }

        /// <summary>
        /// Extracts the generated text from the OpenAI API response
        /// </summary>
        private string ExtractGeneratedText(OpenAIApiResponse openAIResponse)
        {
            if (openAIResponse?.choices != null && openAIResponse.choices.Length > 0)
            {
                var firstChoice = openAIResponse.choices[0];
                return firstChoice?.message?.content?.Trim() ?? "No content generated";
            }
            
            return "No content generated";
        }

        /// <summary>
        /// Parses error messages from failed API responses
        /// </summary>
        private string ParseErrorMessage(int statusCode, string responseBody)
        {
            try
            {
                // Try to parse structured error response
                var errorResponse = JsonConvert.DeserializeObject<OpenAIErrorResponse>(responseBody);
                if (errorResponse?.error?.message != null)
                {
                    return $"OpenAI API Error ({statusCode}): {errorResponse.error.message}";
                }
            }
            catch
            {
                // Fall back to generic error message if parsing fails
            }

            // Return generic error message based on status code
            switch (statusCode)
            {
                case 400:
                    return $"Bad Request ({statusCode}): The request was invalid or malformed";
                case 401:
                    return $"Unauthorized ({statusCode}): Invalid or missing API key";
                case 403:
                    return $"Forbidden ({statusCode}): Access denied to the requested resource";
                case 404:
                    return $"Not Found ({statusCode}): The requested resource was not found";
                case 429:
                    return $"Too Many Requests ({statusCode}): Rate limit exceeded, please try again later";
                case 500:
                    return $"Internal Server Error ({statusCode}): OpenAI service is experiencing issues";
                case 503:
                    return $"Service Unavailable ({statusCode}): OpenAI service is temporarily unavailable";
                default:
                    return $"HTTP Error ({statusCode}): {responseBody}";
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Result structure for API calls
        /// </summary>
        private class ApiCallResult
        {
            public bool IsSuccess { get; set; }
            public int StatusCode { get; set; }
            public string ResponseBody { get; set; }
        }

        /// <summary>
        /// OpenAI API successful response structure
        /// </summary>
        private class OpenAIApiResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public Choice[] choices { get; set; }
            public Usage usage { get; set; }
        }

        private class Choice
        {
            public int index { get; set; }
            public Message message { get; set; }
            public string finish_reason { get; set; }
        }

        private class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        private class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        /// <summary>
        /// OpenAI API error response structure
        /// </summary>
        private class OpenAIErrorResponse
        {
            public ErrorDetail error { get; set; }
        }

        private class ErrorDetail
        {
            public string message { get; set; }
            public string type { get; set; }
            public string param { get; set; }
            public string code { get; set; }
        }

        #endregion
    }
}
