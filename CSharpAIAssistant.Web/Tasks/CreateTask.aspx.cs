using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Tasks
{
    public partial class CreateTask : System.Web.UI.Page
    {
        private AIModelConfigurationService _aiModelConfigService;
        private ApplicationSettingsService _settingsService;
        private AITaskService _aiTaskService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize services
            _aiModelConfigService = new AIModelConfigurationService();
            _settingsService = new ApplicationSettingsService();
            _aiTaskService = new AITaskService();

            // Authorization check - must be authenticated
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account/Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                return;
            }

            if (!IsPostBack)
            {
                PopulateAIModels();
                PopulateDefaultValues();
            }
        }

        /// <summary>
        /// Populates the AI Models dropdown with active models
        /// </summary>
        private void PopulateAIModels()
        {
            try
            {
                var activeModels = _aiModelConfigService.GetActiveModelsForUserSelection();
                
                ddlAIModels.Items.Clear();
                ddlAIModels.Items.Add(new ListItem("-- Select an AI Model --", ""));

                foreach (var model in activeModels)
                {
                    string displayText = $"{model.DisplayName}";
                    if (!string.IsNullOrEmpty(model.Notes))
                    {
                        displayText += $" - {model.Notes}";
                    }
                    
                    ddlAIModels.Items.Add(new ListItem(displayText, model.Id.ToString()));
                }

                if (ddlAIModels.Items.Count == 1)
                {
                    ShowMessage("No AI models are currently available. Please contact an administrator.", "warning");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error populating AI models: {ex.Message}");
                ShowMessage("Error loading AI models. Please try again.", "danger");
            }
        }

        /// <summary>
        /// Populates default values for parameters from application settings
        /// </summary>
        private void PopulateDefaultValues()
        {
            try
            {
                // Set default max tokens
                int defaultMaxTokens = _settingsService.GetIntegerSettingValue("DefaultAITaskMaxTokens", 1000);
                if (defaultMaxTokens > 0)
                {
                    txtMaxTokens.Text = defaultMaxTokens.ToString();
                }

                // Set default temperature
                double defaultTemperature = _settingsService.GetRealSettingValue("DefaultAITaskTemperature", 0.7);
                if (defaultTemperature >= 0.0 && defaultTemperature <= 2.0)
                {
                    txtTemperature.Text = defaultTemperature.ToString("F1");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"Error setting default values: {ex.Message}");
                // Continue without defaults - not critical
            }
        }

        /// <summary>
        /// Handles the Submit Task button click
        /// </summary>
        protected void btnSubmitTask_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (!ValidateInputs())
                {
                    return;
                }

                // Get user ID from session
                if (Session["UserId"] == null)
                {
                    ShowMessage("Session expired. Please log in again.", "danger");
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                int userId = Convert.ToInt32(Session["UserId"]);
                int aiModelConfigurationId = Convert.ToInt32(ddlAIModels.SelectedValue);
                string taskName = string.IsNullOrWhiteSpace(txtTaskName.Text) ? null : txtTaskName.Text.Trim();
                string promptText = txtPrompt.Text.Trim();

                // Parse optional parameters
                int? maxTokensOverride = null;
                if (!string.IsNullOrWhiteSpace(txtMaxTokens.Text))
                {
                    if (int.TryParse(txtMaxTokens.Text, out int maxTokens) && maxTokens > 0)
                    {
                        maxTokensOverride = maxTokens;
                    }
                }

                double? temperatureOverride = null;
                if (!string.IsNullOrWhiteSpace(txtTemperature.Text))
                {
                    if (double.TryParse(txtTemperature.Text, out double temperature) && temperature >= 0.0 && temperature <= 2.0)
                    {
                        temperatureOverride = temperature;
                    }
                }

                // Create the task
                int newTaskId = _aiTaskService.CreateNewTask(
                    userId, 
                    aiModelConfigurationId, 
                    taskName, 
                    promptText, 
                    maxTokensOverride, 
                    temperatureOverride);

                if (newTaskId > 0)
                {
                    ShowMessage($"Task created successfully! Task ID: {newTaskId}. Your task has been queued for processing.", "success");
                    ClearForm();
                    
                    // Optionally redirect to task list after a delay
                    ClientScript.RegisterStartupScript(this.GetType(), "redirect", 
                        "setTimeout(function(){ window.location.href = 'TaskList.aspx'; }, 3000);", true);
                }
                else
                {
                    ShowMessage("Failed to create task. Please try again.", "danger");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error creating task: {ex.Message}");
                ShowMessage("An error occurred while creating your task. Please try again.", "danger");
            }
        }

        /// <summary>
        /// Validates user inputs
        /// </summary>
        /// <returns>True if all inputs are valid, false otherwise</returns>
        private bool ValidateInputs()
        {
            bool isValid = true;
            List<string> errors = new List<string>();

            // Validate AI model selection
            if (string.IsNullOrWhiteSpace(ddlAIModels.SelectedValue))
            {
                errors.Add("Please select an AI model.");
                isValid = false;
            }

            // Validate prompt text
            if (string.IsNullOrWhiteSpace(txtPrompt.Text))
            {
                errors.Add("Please enter a prompt.");
                isValid = false;
            }
            else if (txtPrompt.Text.Trim().Length < 5)
            {
                errors.Add("Prompt must be at least 5 characters long.");
                isValid = false;
            }
            else if (txtPrompt.Text.Trim().Length > 10000)
            {
                errors.Add("Prompt cannot exceed 10,000 characters.");
                isValid = false;
            }

            // Validate max tokens if provided
            if (!string.IsNullOrWhiteSpace(txtMaxTokens.Text))
            {
                if (!int.TryParse(txtMaxTokens.Text, out int maxTokens) || maxTokens <= 0 || maxTokens > 4000)
                {
                    errors.Add("Max tokens must be a number between 1 and 4000.");
                    isValid = false;
                }
            }

            // Validate temperature if provided
            if (!string.IsNullOrWhiteSpace(txtTemperature.Text))
            {
                if (!double.TryParse(txtTemperature.Text, out double temperature) || temperature < 0.0 || temperature > 2.0)
                {
                    errors.Add("Temperature must be a number between 0.0 and 2.0.");
                    isValid = false;
                }
            }

            // Validate task name length if provided
            if (!string.IsNullOrWhiteSpace(txtTaskName.Text) && txtTaskName.Text.Trim().Length > 200)
            {
                errors.Add("Task name cannot exceed 200 characters.");
                isValid = false;
            }

            if (!isValid)
            {
                ShowMessage("Please correct the following errors:<br/>• " + string.Join("<br/>• ", errors), "danger");
            }

            return isValid;
        }

        /// <summary>
        /// Clears the form fields after successful submission
        /// </summary>
        private void ClearForm()
        {
            txtTaskName.Text = string.Empty;
            txtPrompt.Text = string.Empty;
            ddlAIModels.SelectedIndex = 0;
            
            // Reset to default values
            PopulateDefaultValues();
        }

        /// <summary>
        /// Shows a message to the user
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="type">Bootstrap alert type (success, danger, warning, info)</param>
        private void ShowMessage(string message, string type)
        {
            litMessage.Text = message;
            pnlMessage.CssClass = $"alert alert-{type}";
            pnlMessage.Visible = true;
            
            // Auto-hide success messages after 5 seconds
            if (type == "success")
            {
                ClientScript.RegisterStartupScript(this.GetType(), "hideMessage", 
                    "setTimeout(function(){ $('#" + pnlMessage.ClientID + "').fadeOut(); }, 5000);", true);
            }
        }
    }
}
