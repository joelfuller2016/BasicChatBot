using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Admin
{
    public partial class Settings : Page
    {
        private ApplicationSettingsService _settingsService;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verify admin authorization
            if (Session["IsAdmin"] == null || !Convert.ToBoolean(Session["IsAdmin"]))
            {
                Response.Redirect("~/Account/Login.aspx?message=unauthorized");
                return;
            }

            _settingsService = new ApplicationSettingsService();

            if (!IsPostBack)
            {
                BindSettingsGrid();
            }
        }

        protected void chkShowSensitive_CheckedChanged(object sender, EventArgs e)
        {
            pnlSensitiveWarning.Visible = chkShowSensitive.Checked;
            BindSettingsGrid();
        }

        protected void gvSettings_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvSettings.EditIndex = e.NewEditIndex;
            BindSettingsGrid();
        }

        protected void gvSettings_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvSettings.EditIndex = -1;
            BindSettingsGrid();
        }

        protected void gvSettings_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                string settingKey = gvSettings.DataKeys[e.RowIndex].Value.ToString();
                GridViewRow row = gvSettings.Rows[e.RowIndex];
                
                // Find the value textbox
                TextBox txtValue = row.FindControl("txtValue") as TextBox;
                if (txtValue == null)
                {
                    ShowError("Could not find value control for updating.");
                    return;
                }

                // Get the original setting
                var originalSetting = _settingsService.GetAllSettingsForAdminView()
                    .FirstOrDefault(s => s.SettingKey == settingKey);

                if (originalSetting == null)
                {
                    ShowError($"Setting '{settingKey}' not found.");
                    return;
                }

                // Prepare updated setting
                var updatedSetting = new ApplicationSetting
                {
                    SettingKey = originalSetting.SettingKey,
                    SettingValue = txtValue.Text.Trim(),
                    SettingDescription = originalSetting.SettingDescription,
                    DataType = originalSetting.DataType,
                    IsSensitive = originalSetting.IsSensitive,
                    GroupName = originalSetting.GroupName
                };

                // Save the setting (encryption will be handled automatically if sensitive)
                _settingsService.SaveSetting(updatedSetting);

                // Reset edit mode and rebind
                gvSettings.EditIndex = -1;
                BindSettingsGrid();

                ShowSuccess($"Setting '{settingKey}' updated successfully.");

                System.Diagnostics.Trace.WriteLine($"Admin updated setting: {settingKey} by user {Session["Username"]}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error updating setting: {0}", ex.ToString());
                ShowError("An error occurred while updating the setting. Please try again.");
            }
        }

        protected void gvSettings_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var setting = e.Row.DataItem as ApplicationSetting;
                if (setting == null) return;

                // Handle value display based on sensitivity and show/hide setting
                if ((e.Row.RowState & DataControlRowState.Edit) == 0)
                {
                    // Display mode
                    Literal litValue = e.Row.FindControl("litValue") as Literal;
                    if (litValue != null)
                    {
                        if (setting.IsSensitive)
                        {
                            if (chkShowSensitive.Checked)
                            {
                                // Show decrypted value
                                string decryptedValue = _settingsService.GetDecryptedSettingValue(setting.SettingKey, "[Decryption Failed]");
                                litValue.Text = $"<span class='text-danger font-weight-bold'>{Server.HtmlEncode(decryptedValue)}</span>";
                            }
                            else
                            {
                                // Show masked value
                                litValue.Text = "<span class='text-muted'>********</span>";
                            }
                        }
                        else
                        {
                            // Show plain value
                            litValue.Text = Server.HtmlEncode(setting.SettingValue ?? "");
                        }
                    }
                }
                else
                {
                    // Edit mode
                    TextBox txtValue = e.Row.FindControl("txtValue") as TextBox;
                    if (txtValue != null)
                    {
                        if (setting.IsSensitive && chkShowSensitive.Checked)
                        {
                            // Pre-populate with decrypted value for editing
                            string decryptedValue = _settingsService.GetDecryptedSettingValue(setting.SettingKey, "");
                            txtValue.Text = decryptedValue;
                            txtValue.CssClass += " border-danger";
                            txtValue.ToolTip = "This is a sensitive value. Enter new value in plaintext - it will be encrypted automatically.";
                        }
                        else if (setting.IsSensitive)
                        {
                            // Show placeholder for sensitive values when not showing decrypted
                            txtValue.Text = "";
                            txtValue.Attributes["placeholder"] = "Enter new sensitive value (will be encrypted)";
                            txtValue.CssClass += " border-warning";
                            txtValue.ToolTip = "Enter new sensitive value in plaintext - it will be encrypted automatically.";
                        }
                        else
                        {
                            // Regular value
                            txtValue.Text = setting.SettingValue ?? "";
                        }
                    }
                }
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            gvSettings.EditIndex = -1;
            BindSettingsGrid();
            ShowSuccess("Settings refreshed successfully.");
        }

        protected void btnTestEncryption_Click(object sender, EventArgs e)
        {
            try
            {
                bool isValid = EncryptionService.ValidateEncryptionService();
                if (isValid)
                {
                    ShowSuccess("Encryption service is working correctly.");
                }
                else
                {
                    ShowError("Encryption service validation failed. Check system logs for details.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Encryption test error: {0}", ex.ToString());
                ShowError("Error testing encryption service: " + ex.Message);
            }
        }

        private void BindSettingsGrid()
        {
            try
            {
                var settings = _settingsService.GetAllSettingsForAdminView()
                    .OrderBy(s => s.GroupName)
                    .ThenBy(s => s.SettingKey)
                    .ToList();

                gvSettings.DataSource = settings;
                gvSettings.DataBind();

                // Update sensitive warning visibility
                pnlSensitiveWarning.Visible = chkShowSensitive.Checked;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error binding settings grid: {0}", ex.ToString());
                ShowError("Error loading settings. Please refresh the page.");
            }
        }

        private void ShowError(string message)
        {
            litError.Text = message;
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            litSuccess.Text = message;
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
        }
    }
}
