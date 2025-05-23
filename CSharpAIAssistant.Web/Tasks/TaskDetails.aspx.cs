using System;
using System.Web.UI;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Tasks
{
    public partial class TaskDetails : System.Web.UI.Page
    {
        private AITaskService _aiTaskService;
        private AIModelConfigurationService _aiModelService;
        private int _taskId;
        private AITask _currentTask;
        private AITaskResult _currentResult;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            // Initialize services
            _aiTaskService = new AITaskService();
            _aiModelService = new AIModelConfigurationService();

            // Get task ID from query string
            if (!int.TryParse(Request.QueryString["taskId"], out _taskId))
            {
                ShowError("Invalid task ID specified.");
                return;
            }

            if (!IsPostBack)
            {
                LoadTaskDetails();
            }
        }

        private void LoadTaskDetails()
        {
            try
            {
                // Get current user ID
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    ShowError("Unable to determine current user.");
                    return;
                }

                // Check if user is admin
                bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

                // Get task and result
                var taskAndResult = _aiTaskService.GetTaskDetailsWithResult(_taskId, userId);
                if (taskAndResult == null)
                {
                    ShowError("Task not found or you don't have permission to view it.");
                    return;
                }

                _currentTask = taskAndResult.Item1;
                _currentResult = taskAndResult.Item2;

                if (_currentTask == null)
                {
                    ShowError("Task not found.");
                    return;
                }

                // If not admin and task doesn't belong to user, deny access
                if (!isAdmin && _currentTask.UserId != userId)
                {
                    ShowError("You don't have permission to view this task.");
                    return;
                }

                DisplayTaskDetails();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading task details: {ex.Message}");
            }
        }

        private void DisplayTaskDetails()
        {
            // Show task info panel
            pnlTaskInfo.Visible = true;

            // Basic task information
            litTaskId.Text = _currentTask.Id.ToString();
            litTaskName.Text = string.IsNullOrEmpty(_currentTask.TaskName) ? "Untitled Task" : _currentTask.TaskName;
            litStatus.Text = _currentTask.Status;
            litPromptText.Text = _currentTask.PromptText;

            // Set status badge CSS class
            statusBadge.Attributes["class"] = $"task-status {GetStatusCssClass(_currentTask.Status)}";

            // Get AI model information
            try
            {
                var aiModel = _aiModelService.GetModelById(_currentTask.AIModelConfigurationId);
                litAIModel.Text = aiModel != null ? $"{aiModel.DisplayName} ({aiModel.ModelIdentifier})" : "Unknown Model";
            }
            catch
            {
                litAIModel.Text = "Unknown Model";
            }

            // Task parameters
            litMaxTokens.Text = _currentTask.MaxTokens?.ToString() ?? "Default";
            litTemperature.Text = _currentTask.Temperature?.ToString("F1") ?? "Default";

            // Timestamps
            litCreatedAt.Text = _currentTask.CreatedAt.ToString("MMM dd, yyyy HH:mm:ss");
            litQueuedAt.Text = _currentTask.QueuedAt?.ToString("MMM dd, yyyy HH:mm:ss") ?? "-";
            litProcessingStartedAt.Text = _currentTask.ProcessingStartedAt?.ToString("MMM dd, yyyy HH:mm:ss") ?? "-";
            litCompletedAt.Text = _currentTask.CompletedAt?.ToString("MMM dd, yyyy HH:mm:ss") ?? "-";

            // Display results based on task status
            if (_currentTask.Status == "Processing" || _currentTask.Status == "Queued")
            {
                pnlProcessing.Visible = true;
                pnlResults.Visible = false;
            }
            else if (_currentTask.Status == "Completed" || _currentTask.Status == "Failed")
            {
                pnlProcessing.Visible = false;
                pnlResults.Visible = true;

                if (_currentResult != null)
                {
                    if (_currentResult.Success && _currentTask.Status == "Completed")
                    {
                        // Show successful result
                        pnlSuccessResult.Visible = true;
                        pnlErrorResult.Visible = false;
                        
                        litGeneratedContent.Text = _currentResult.GeneratedContent ?? "No content generated.";

                        // Show token usage if available
                        if (_currentResult.TokensUsed_Total.HasValue)
                        {
                            pnlTokenUsage.Visible = true;
                            litPromptTokens.Text = _currentResult.TokensUsed_Prompt?.ToString() ?? "0";
                            litCompletionTokens.Text = _currentResult.TokensUsed_Completion?.ToString() ?? "0";
                            litTotalTokens.Text = _currentResult.TokensUsed_Total?.ToString() ?? "0";
                            litProcessingTime.Text = _currentResult.ProcessingTimeMs?.ToString() ?? "0";
                        }
                    }
                    else
                    {
                        // Show error result
                        pnlSuccessResult.Visible = false;
                        pnlErrorResult.Visible = true;
                        
                        string errorMessage = "";
                        if (!string.IsNullOrEmpty(_currentTask.ErrorMessage))
                        {
                            errorMessage = _currentTask.ErrorMessage;
                        }
                        else if (_currentResult != null && !string.IsNullOrEmpty(_currentResult.GeneratedContent))
                        {
                            errorMessage = _currentResult.GeneratedContent;
                        }
                        else
                        {
                            errorMessage = "The task failed without providing specific error details.";
                        }
                        
                        litErrorDetails.Text = errorMessage;
                    }
                }
                else
                {
                    // No result available but task is marked as completed/failed
                    pnlSuccessResult.Visible = false;
                    pnlErrorResult.Visible = true;
                    litErrorDetails.Text = _currentTask.ErrorMessage ?? "Task completed but no result data is available.";
                }
            }
            else
            {
                // Task is in Pending state or other status
                pnlProcessing.Visible = false;
                pnlResults.Visible = false;
            }
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            pnlTaskInfo.Visible = false;
            litErrorMessage.Text = message;
        }

        private int GetCurrentUserId()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int userId))
            {
                return userId;
            }
            return 0;
        }

        private string GetStatusCssClass(string status)
        {
            switch (status?.ToLower())
            {
                case "pending":
                    return "status-pending";
                case "queued":
                    return "status-queued";
                case "processing":
                    return "status-processing";
                case "completed":
                    return "status-completed";
                case "failed":
                    return "status-failed";
                default:
                    return "status-pending";
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTaskDetails();
        }

        // Helper method for JavaScript auto-refresh logic
        protected bool IsTaskProcessing()
        {
            return _currentTask != null && 
                   (_currentTask.Status == "Processing" || 
                    _currentTask.Status == "Queued" || 
                    _currentTask.Status == "Pending");
        }
    }
}
