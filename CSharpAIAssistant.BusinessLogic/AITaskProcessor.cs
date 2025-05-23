using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;
using CSharpAIAssistant.Models.Interfaces;

namespace CSharpAIAssistant.BusinessLogic
{
    /// <summary>
    /// Background task processor for handling AI tasks asynchronously
    /// Uses a dedicated thread with a queue and semaphore for efficient processing
    /// </summary>
    public static class AITaskProcessor
    {
        #region Private Fields
        
        private static readonly ConcurrentQueue<int> _taskQueue = new ConcurrentQueue<int>();
        private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private static Thread _processorThread;
        private static volatile bool _shuttingDown = false;
        private static readonly object _lockObject = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether the processor is currently running
        /// </summary>
        public static bool IsRunning
        {
            get
            {
                lock (_lockObject)
                {
                    return _processorThread != null && _processorThread.IsAlive && !_shuttingDown;
                }
            }
        }

        /// <summary>
        /// Gets the number of tasks currently in the queue
        /// </summary>
        public static int QueueCount => _taskQueue.Count;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the AI task processor background service
        /// Called from Global.asax Application_Start
        /// </summary>
        public static void Start()
        {
            lock (_lockObject)
            {
                if (_processorThread == null || !_processorThread.IsAlive)
                {
                    _shuttingDown = false;
                    _processorThread = new Thread(ProcessorLoop)
                    {
                        IsBackground = true,
                        Name = "AITaskProcessor"
                    };
                    _processorThread.Start();
                    
                    Trace.WriteLine($"AITaskProcessor started successfully on thread {_processorThread.ManagedThreadId}");
                }
                else
                {
                    Trace.WriteLine("AITaskProcessor is already running");
                }
            }
        }

        /// <summary>
        /// Stops the AI task processor background service
        /// Called from Global.asax Application_End
        /// </summary>
        public static void Stop()
        {
            lock (_lockObject)
            {
                if (_processorThread != null && _processorThread.IsAlive)
                {
                    Trace.WriteLine("Stopping AITaskProcessor...");
                    
                    _shuttingDown = true;
                    _signal.Release(); // Wake up the processor thread so it can exit
                    
                    // Wait for the thread to finish (with timeout)
                    bool stopped = _processorThread.Join(TimeSpan.FromSeconds(10));
                    
                    if (stopped)
                    {
                        Trace.WriteLine("AITaskProcessor stopped successfully");
                    }
                    else
                    {
                        Trace.WriteLine("AITaskProcessor stop timeout - forcing abort");
                        _processorThread.Abort();
                    }
                    
                    _processorThread = null;
                }
                else
                {
                    Trace.WriteLine("AITaskProcessor was not running");
                }
            }
        }

        /// <summary>
        /// Queues a task for processing
        /// </summary>
        /// <param name="taskId">ID of the task to queue</param>
        public static void QueueTask(int taskId)
        {
            if (_shuttingDown)
            {
                Trace.WriteLine($"Cannot queue task {taskId} - processor is shutting down");
                return;
            }

            _taskQueue.Enqueue(taskId);
            _signal.Release(); // Signal that a new task is available
            
            Trace.WriteLine($"Task {taskId} queued for processing. Queue count: {_taskQueue.Count}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Main processing loop running on the background thread
        /// </summary>
        private static void ProcessorLoop()
        {
            Trace.WriteLine("AITaskProcessor loop started");

            // Initialize services (thread-safe instantiation)
            AITaskDAL aiTaskDAL = null;
            AITaskResultDAL aiTaskResultDAL = null;
            AIModelConfigurationService aiModelConfigService = null;
            ApplicationSettingsService settingsService = null;
            IAIService aiService = null;

            try
            {
                // Initialize dependencies
                aiTaskDAL = new AITaskDAL();
                aiTaskResultDAL = new AITaskResultDAL();
                aiModelConfigService = new AIModelConfigurationService();
                settingsService = new ApplicationSettingsService();
                
                // Initialize AI service based on configuration
                bool useMockService = settingsService.GetBooleanSettingValue("UseMockAIService", true);
                if (useMockService)
                {
                    aiService = new MockAIService();
                    Trace.WriteLine("AITaskProcessor initialized with MockAIService");
                }
                else
                {
                    aiService = new OpenAIService();
                    Trace.WriteLine("AITaskProcessor initialized with OpenAIService");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to initialize AITaskProcessor dependencies: {ex.Message}");
                return;
            }

            // Main processing loop
            while (!_shuttingDown)
            {
                try
                {
                    // Wait for a signal that work is available
                    _signal.Wait();
                    
                    // Check if we're shutting down
                    if (_shuttingDown)
                    {
                        break;
                    }

                    // Process all available tasks
                    while (_taskQueue.TryDequeue(out int taskId) && !_shuttingDown)
                    {
                        ProcessSingleTask(taskId, aiTaskDAL, aiTaskResultDAL, aiModelConfigService, settingsService, aiService);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error in AITaskProcessor main loop: {ex.Message}");
                    // Continue processing - don't let one error stop the entire processor
                    Thread.Sleep(1000); // Brief pause to prevent tight error loops
                }
            }

            Trace.WriteLine("AITaskProcessor loop ended");
        }

        /// <summary>
        /// Processes a single AI task
        /// </summary>
        private static void ProcessSingleTask(int taskId, AITaskDAL aiTaskDAL, AITaskResultDAL aiTaskResultDAL, 
            AIModelConfigurationService aiModelConfigService, ApplicationSettingsService settingsService, IAIService aiService)
        {
            Stopwatch processingStopwatch = Stopwatch.StartNew();
            AITask task = null;
            
            try
            {
                Trace.WriteLine($"Processing task {taskId}...");

                // 1. Fetch the task
                task = aiTaskDAL.GetById(taskId);
                if (task == null)
                {
                    Trace.TraceWarning($"Task {taskId} not found in database");
                    return;
                }

                // 2. Verify task is in correct state
                if (task.Status != "Queued")
                {
                    Trace.TraceWarning($"Task {taskId} has status '{task.Status}' instead of 'Queued'");
                    return;
                }

                // 3. Update status to Processing
                bool statusUpdated = aiTaskDAL.UpdateStatus(taskId, "Processing", DateTime.UtcNow);
                if (!statusUpdated)
                {
                    Trace.TraceError($"Failed to update task {taskId} status to Processing");
                    return;
                }

                // 4. Fetch AI model configuration
                AIModelConfiguration modelConfig = aiModelConfigService.GetModelById(task.AIModelConfigurationId);
                if (modelConfig == null || !modelConfig.IsActive)
                {
                    string error = $"AI model configuration {task.AIModelConfigurationId} not found or inactive";
                    aiTaskDAL.UpdateStatus(taskId, "Failed", DateTime.UtcNow, error);
                    Trace.TraceError($"Task {taskId}: {error}");
                    return;
                }

                // 5. Fetch API key
                string apiKey = settingsService.GetOpenAIApiKey(modelConfig.OpenAISettingKeyForApiKey);
                if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("placeholder") || apiKey.Contains("YOUR_"))
                {
                    string error = "API key not configured or is a placeholder value";
                    aiTaskDAL.UpdateStatus(taskId, "Failed", DateTime.UtcNow, error);
                    Trace.TraceError($"Task {taskId}: {error}");
                    return;
                }

                // 6. Process the task using the AI service
                Trace.WriteLine($"Task {taskId}: Calling AI service with model {modelConfig.ModelIdentifier}");
                AIResponse aiResponse = aiService.ProcessPrompt(task, modelConfig, apiKey);

                // 7. Create and save the result
                AITaskResult result = new AITaskResult
                {
                    AITaskId = taskId,
                    GeneratedContent = aiResponse.GeneratedText,
                    TokensUsed_Prompt = aiResponse.TokensUsedPrompt,
                    TokensUsed_Completion = aiResponse.TokensUsedCompletion,
                    TokensUsed_Total = aiResponse.TokensUsedTotal,
                    ProcessingTimeMs = aiResponse.ProcessingTimeMs ?? (int)processingStopwatch.ElapsedMilliseconds,
                    ModelUsedIdentifier = aiResponse.ModelUsed ?? modelConfig.ModelIdentifier,
                    Success = aiResponse.Success,
                    CreatedAt = DateTime.UtcNow
                };

                bool resultSaved = aiTaskResultDAL.Insert(result);
                if (!resultSaved)
                {
                    Trace.TraceError($"Failed to save result for task {taskId}");
                }

                // 8. Update task status based on AI response
                if (aiResponse.Success)
                {
                    aiTaskDAL.UpdateStatus(taskId, "Completed", DateTime.UtcNow);
                    Trace.WriteLine($"Task {taskId} completed successfully in {processingStopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    string errorMessage = aiResponse.ErrorMessage ?? "AI processing failed with unknown error";
                    aiTaskDAL.UpdateStatus(taskId, "Failed", DateTime.UtcNow, errorMessage);
                    Trace.TraceError($"Task {taskId} failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during task processing
                try
                {
                    string errorMessage = $"Unexpected error during processing: {ex.Message}";
                    aiTaskDAL?.UpdateStatus(taskId, "Failed", DateTime.UtcNow, errorMessage);
                    Trace.TraceError($"Task {taskId} failed with exception: {ex}");
                }
                catch (Exception innerEx)
                {
                    Trace.TraceError($"Failed to update error status for task {taskId}: {innerEx.Message}");
                }
            }
            finally
            {
                processingStopwatch.Stop();
            }
        }

        #endregion
    }
}
