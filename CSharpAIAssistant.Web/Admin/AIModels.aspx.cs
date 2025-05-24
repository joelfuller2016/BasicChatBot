using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Admin
{
    public partial class AIModels : System.Web.UI.Page
    {
        private AIModelConfigurationService _aiModelService;
        private ApplicationSettingsService _settingsService;
        private int _editingModelId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check admin authentication
            if (!User.Identity.IsAuthenticated || Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            // Initialize services
            _aiModelService = new AIModelConfigurationService();
            _settingsService = new ApplicationSettingsService();

            if (!IsPostBack)
            {
                PopulateApiKeyDropdown();
                BindModelsGrid();
            }
        }

        private void PopulateApiKeyDropdown()
        {
            try
            {
                // Get all encrypted settings that could be API keys
                var allSettings = _settingsService.GetAllSettingsForAdminView();
                var apiKeySettings = allSettings
                    .Where(s => s.DataType == "EncryptedString" && 
                               (s.GroupName == "AI" || 
                                s.SettingKey.ToLower().Contains("apikey") || 
                                s.SettingKey.ToLower().Contains("api_key")))
                    .OrderBy(s => s.SettingKey)
                    .ToList();

                ddlApiKeySetting.Items.Clear();
                ddlApiKeySetting.Items.Add(new ListItem("-- Select API Key Setting --", ""));

                foreach (var setting in apiKeySettings)
                {
                    string displayText = setting.SettingKey;
                    if (!string.IsNullOrEmpty(setting.SettingDescription))
                    {
                        displayText += $" ({setting.SettingDescription})";
                    }
                    ddlApiKeySetting.Items.Add(new ListItem(displayText, setting.SettingKey));
                }

                if (apiKeySettings.Count == 0)
                {
                    ddlApiKeySetting.Items.Add(new ListItem("No encrypted API key settings found", ""));
                    ShowMessage("No API key settings found. Please add encrypted API key settings in Application Settings first.", "warning");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading API key settings: {ex.Message}", "danger");
            }
        }

        private void BindModelsGrid()
        {
            try
            {
                var models = _aiModelService.GetAllModelsForAdmin();
                gvModels.DataSource = models;
                gvModels.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading AI models: {ex.Message}", "danger");
            }
        }

        protected void btnShowAddForm_Click(object sender, EventArgs e)
        {
            ShowAddForm();
        }

        private void ShowAddForm()
        {
            ClearForm();
            _editingModelId = 0;
            litFormTitle.Text = "Add New AI Model";
            btnSaveModel.Text = "Add Model";
            pnlAddModel.Visible = false;
            pnlModelForm.Visible = true;
            PopulateApiKeyDropdown(); // Refresh in case settings changed
        }

        private void ShowEditForm(int modelId)
        {
            try
            {
                var model = _aiModelService.GetModelById(modelId);
                if (model == null)
                {
                    ShowMessage("Model not found.", "danger");
                    return;
                }

                _editingModelId = modelId;
                litFormTitle.Text = "Edit AI Model";
                btnSaveModel.Text = "Update Model";

                // Populate form with model data
                txtDisplayName.Text = model.DisplayName;
                txtModelIdentifier.Text = model.ModelIdentifier;
                txtDefaultMaxTokens.Text = model.DefaultMaxTokens?.ToString() ?? "";
                txtDefaultTemperature.Text = model.DefaultTemperature?.ToString("F1") ?? "";
                txtNotes.Text = model.Notes ?? "";
                chkIsActive.Checked = model.IsActive;

                PopulateApiKeyDropdown();
                
                // Set selected API key setting
                var apiKeyItem = ddlApiKeySetting.Items.FindByValue(model.OpenAISettingKeyForApiKey);
                if (apiKeyItem != null)
                {
                    ddlApiKeySetting.SelectedValue = model.OpenAISettingKeyForApiKey;
                }

                pnlAddModel.Visible = false;
                pnlModelForm.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading model for editing: {ex.Message}", "danger");
            }
        }

        protected void btnSaveModel_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(txtDisplayName.Text))
                {
                    ShowMessage("Display Name is required.", "danger");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtModelIdentifier.Text))
                {
                    ShowMessage("Model Identifier is required.", "danger");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ddlApiKeySetting.SelectedValue))
                {
                    ShowMessage("Please select an API Key Setting.", "danger");
                    return;
                }

                // Parse numeric values
                int? maxTokens = null;
                if (!string.IsNullOrWhiteSpace(txtDefaultMaxTokens.Text))
                {
                    if (int.TryParse(txtDefaultMaxTokens.Text, out int tokens) && tokens > 0)
                    {
                        maxTokens = Math.Min(tokens, 4000); // Cap at 4000 tokens
                    }
                    else
                    {
                        ShowMessage("Default Max Tokens must be a positive number.", "danger");
                        return;
                    }
                }

                double? temperature = null;
                if (!string.IsNullOrWhiteSpace(txtDefaultTemperature.Text))
                {
                    if (double.TryParse(txtDefaultTemperature.Text, out double temp) && temp >= 0.0 && temp <= 2.0)
                    {
                        temperature = temp;
                    }
                    else
                    {
                        ShowMessage("Default Temperature must be between 0.0 and 2.0.", "danger");
                        return;
                    }
                }

                // Create or update model
                AIModelConfiguration model;
                bool isNewModel = _editingModelId == 0;

                if (isNewModel)
                {
                    // Check for duplicate model identifier
                    var existingModels = _aiModelService.GetAllModelsForAdmin();
                    if (existingModels.Any(m => m.ModelIdentifier.Equals(txtModelIdentifier.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        ShowMessage("A model with this identifier already exists.", "danger");
                        return;
                    }

                    model = new AIModelConfiguration
                    {
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    model = _aiModelService.GetModelById(_editingModelId);
                    if (model == null)
                    {
                        ShowMessage("Model not found.", "danger");
                        return;
                    }
                    model.UpdatedAt = DateTime.UtcNow;
                }

                // Set model properties
                model.DisplayName = txtDisplayName.Text.Trim();
                model.ModelIdentifier = txtModelIdentifier.Text.Trim();
                model.OpenAISettingKeyForApiKey = ddlApiKeySetting.SelectedValue;
                model.DefaultMaxTokens = maxTokens;
                model.DefaultTemperature = temperature;
                model.IsActive = chkIsActive.Checked;
                model.Notes = txtNotes.Text.Trim();

                // Save model
                bool success = _aiModelService.SaveModelConfiguration(model);
                
                if (success)
                {
                    string action = isNewModel ? "added" : "updated";
                    ShowMessage($"AI model '{model.DisplayName}' has been {action} successfully.", "success");
                    HideForm();
                    BindModelsGrid();
                }
                else
                {
                    ShowMessage("Failed to save the AI model. Please try again.", "danger");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error saving AI model: {ex.Message}", "danger");
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            HideForm();
        }

        private void HideForm()
        {
            pnlModelForm.Visible = false;
            pnlAddModel.Visible = true;
            ClearForm();
        }

        private void ClearForm()
        {
            txtDisplayName.Text = "";
            txtModelIdentifier.Text = "";
            txtDefaultMaxTokens.Text = "";
            txtDefaultTemperature.Text = "";
            txtNotes.Text = "";
            chkIsActive.Checked = true;
            ddlApiKeySetting.SelectedIndex = 0;
            _editingModelId = 0;
        }

        protected void gvModels_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (int.TryParse(e.CommandArgument.ToString(), out int modelId))
            {
                switch (e.CommandName)
                {
                    case "EditModel":
                        ShowEditForm(modelId);
                        break;

                    case "ToggleStatus":
                        ToggleModelStatus(modelId);
                        break;

                    case "DeleteModel":
                        DeleteModel(modelId);
                        break;
                }
            }
        }

        protected void gvModels_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var model = (AIModelConfiguration)e.Row.DataItem;
                
                // Find the toggle status button and set its properties
                var btnToggleStatus = (Button)e.Row.FindControl("btnToggleStatus");
                if (btnToggleStatus != null)
                {
                    if (model.IsActive)
                    {
                        btnToggleStatus.Text = "Deactivate";
                        btnToggleStatus.CssClass = "btn btn-warning btn-sm";
                    }
                    else
                    {
                        btnToggleStatus.Text = "Activate";
                        btnToggleStatus.CssClass = "btn btn-success btn-sm";
                    }
                }
            }
        }

        private void ToggleModelStatus(int modelId)
        {
            try
            {
                var model = _aiModelService.GetModelById(modelId);
                if (model == null)
                {
                    ShowMessage("Model not found.", "danger");
                    return;
                }

                model.IsActive = !model.IsActive;
                model.UpdatedAt = DateTime.UtcNow;

                bool success = _aiModelService.SaveModelConfiguration(model);
                
                if (success)
                {
                    string status = model.IsActive ? "activated" : "deactivated";
                    ShowMessage($"Model '{model.DisplayName}' has been {status}.", "success");
                    BindModelsGrid();
                }
                else
                {
                    ShowMessage("Failed to update model status.", "danger");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error updating model status: {ex.Message}", "danger");
            }
        }

        private void DeleteModel(int modelId)
        {
            try
            {
                var model = _aiModelService.GetModelById(modelId);
                if (model == null)
                {
                    ShowMessage("Model not found.", "danger");
                    return;
                }

                // Note: For now we'll just deactivate the model instead of hard delete
                // to preserve referential integrity with existing tasks
                model.IsActive = false;
                model.UpdatedAt = DateTime.UtcNow;

                bool success = _aiModelService.SaveModelConfiguration(model);
                
                if (success)
                {
                    ShowMessage($"Model '{model.DisplayName}' has been deactivated (soft delete).", "info");
                    BindModelsGrid();
                }
                else
                {
                    ShowMessage("Failed to delete model.", "danger");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error deleting model: {ex.Message}", "danger");
            }
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = $"alert alert-{type}";
            litMessage.Text = message;
        }
    }
}
