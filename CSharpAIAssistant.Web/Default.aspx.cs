using System;
using System.Web.UI;
using CSharpAIAssistant.BusinessLogic;

namespace CSharpAIAssistant.Web
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPageContent();
            }
        }

        private void LoadPageContent()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    LoadAuthenticatedUserContent();
                }
                else
                {
                    LoadGuestContent();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error loading default page content: {0}", ex.ToString());
                // Show guest content as fallback
                LoadGuestContent();
            }
        }

        private void LoadAuthenticatedUserContent()
        {
            // Show authenticated panels
            pnlAuthenticated.Visible = true;
            pnlUserDashboard.Visible = true;
            pnlGuest.Visible = false;

            // Get user information from session
            string username = Session["Username"]?.ToString() ?? User.Identity.Name ?? "User";
            int userId = Convert.ToInt32(Session["UserId"] ?? 0);
            bool isAdmin = Convert.ToBoolean(Session["IsAdmin"] ?? false);

            // Set username
            litUsername.Text = Server.HtmlEncode(username);

            // Show admin notice if applicable
            if (isAdmin)
            {
                pnlAdminNotice.Visible = true;
            }

            // Load user statistics if user ID is available
            if (userId > 0)
            {
                LoadUserStatistics(userId);
            }

            // Load available models count
            LoadAvailableModelsCount();
        }

        private void LoadGuestContent()
        {
            // Show guest panels only
            pnlAuthenticated.Visible = false;
            pnlUserDashboard.Visible = false;
            pnlGuest.Visible = true;
            pnlAdminNotice.Visible = false;
        }

        private void LoadUserStatistics(int userId)
        {
            try
            {
                var aiTaskService = new AITaskService();
                
                // Get user statistics
                var (totalTasks, successfulTasks, totalTokensUsed) = aiTaskService.GetUserStatistics(userId);
                
                // Update UI
                litTotalTasks.Text = totalTasks.ToString();
                litCompletedTasks.Text = successfulTasks.ToString();
                litTotalTokens.Text = FormatTokenCount(totalTokensUsed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error loading user statistics for user {0}: {1}", userId, ex.ToString());
                
                // Set default values on error
                litTotalTasks.Text = "—";
                litCompletedTasks.Text = "—";
                litTotalTokens.Text = "—";
            }
        }

        private void LoadAvailableModelsCount()
        {
            try
            {
                var aiModelService = new AIModelConfigurationService();
                var activeModels = aiModelService.GetActiveModelsForUserSelection();
                
                litAvailableModels.Text = activeModels.Count.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error loading available models count: {0}", ex.ToString());
                litAvailableModels.Text = "—";
            }
        }

        /// <summary>
        /// Formats token count for display with appropriate units
        /// </summary>
        /// <param name="tokenCount">Number of tokens</param>
        /// <returns>Formatted string</returns>
        private string FormatTokenCount(long tokenCount)
        {
            if (tokenCount >= 1000000)
            {
                return $"{tokenCount / 1000000.0:F1}M";
            }
            else if (tokenCount >= 1000)
            {
                return $"{tokenCount / 1000.0:F1}K";
            }
            else
            {
                return tokenCount.ToString();
            }
        }
    }
}
