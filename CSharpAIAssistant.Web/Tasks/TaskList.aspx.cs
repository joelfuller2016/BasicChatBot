using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Tasks
{
    public partial class TaskList : System.Web.UI.Page
    {
        private AITaskService _aiTaskService;
        private AIModelConfigurationService _aiModelConfigService;
        private int _currentPage = 1;
        private int _pageSize = 20;
        private string _statusFilter = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize services
            _aiTaskService = new AITaskService();
            _aiModelConfigService = new AIModelConfigurationService();

            // Authorization check - must be authenticated
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account/Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                return;
            }

            // Get pagination and filter parameters
            if (int.TryParse(Request.QueryString["page"], out int page) && page > 0)
            {
                _currentPage = page;
            }

            if (int.TryParse(Request.QueryString["pageSize"], out int pageSize) && pageSize > 0)
            {
                _pageSize = pageSize;
                ddlPageSize.SelectedValue = pageSize.ToString();
            }

            _statusFilter = Request.QueryString["status"] ?? "";
            if (!string.IsNullOrEmpty(_statusFilter))
            {
                ddlStatusFilter.SelectedValue = _statusFilter;
            }

            if (!IsPostBack)
            {
                LoadTaskData();
                LoadTaskStatistics();
            }
        }

        /// <summary>
        /// Loads the task data for the current user
        /// </summary>
        private void LoadTaskData()
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                int userId = Convert.ToInt32(Session["UserId"]);

                // Get tasks with current filters
                var tasks = _aiTaskService.GetUserTasksWithFilters(userId, _currentPage, _pageSize, _statusFilter);
                
                if (tasks != null && tasks.Any())
                {
                    gvTasks.DataSource = tasks;
                    gvTasks.DataBind();
                    
                    // Generate pagination
                    GeneratePagination(userId);
                }
                else
                {
                    gvTasks.DataSource = new List<AITask>();
                    gvTasks.DataBind();
                    litPagination.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error loading task data: {ex.Message}");
                // Show user-friendly error
                gvTasks.DataSource = new List<AITask>();
                gvTasks.DataBind();
                litPagination.Text = "<div class='alert alert-danger'>Error loading tasks. Please try again.</div>";
            }
        }

        /// <summary>
        /// Loads task statistics for the dashboard
        /// </summary>
        private void LoadTaskStatistics()
        {
            try
            {
                if (Session["UserId"] == null) return;

                int userId = Convert.ToInt32(Session["UserId"]);
                var stats = _aiTaskService.GetUserTaskStatistics(userId);

                litTotalTasks.Text = stats.TotalTasks.ToString();
                litCompletedTasks.Text = stats.CompletedTasks.ToString();
                litProcessingTasks.Text = (stats.PendingTasks + stats.QueuedTasks + stats.ProcessingTasks).ToString();
                litFailedTasks.Text = stats.FailedTasks.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error loading task statistics: {ex.Message}");
                // Set default values
                litTotalTasks.Text = "0";
                litCompletedTasks.Text = "0";
                litProcessingTasks.Text = "0";
                litFailedTasks.Text = "0";
            }
        }

        /// <summary>
        /// Generates pagination controls
        /// </summary>
        private void GeneratePagination(int userId)
        {
            try
            {
                int totalTasks = _aiTaskService.GetUserTaskCount(userId, _statusFilter);
                int totalPages = (int)Math.Ceiling((double)totalTasks / _pageSize);

                if (totalPages <= 1)
                {
                    litPagination.Text = "";
                    return;
                }

                StringBuilder pagination = new StringBuilder();
                pagination.Append("<nav aria-label='Tasks pagination'>");
                pagination.Append("<ul class='pagination pagination-lg'>");

                // Previous button
                if (_currentPage > 1)
                {
                    string prevUrl = GetPageUrl(_currentPage - 1);
                    pagination.Append($"<li class='page-item'><a class='page-link' href='{prevUrl}'>Previous</a></li>");
                }
                else
                {
                    pagination.Append("<li class='page-item disabled'><span class='page-link'>Previous</span></li>");
                }

                // Page numbers
                int startPage = Math.Max(1, _currentPage - 2);
                int endPage = Math.Min(totalPages, _currentPage + 2);

                for (int i = startPage; i <= endPage; i++)
                {
                    if (i == _currentPage)
                    {
                        pagination.Append($"<li class='page-item active'><span class='page-link'>{i}</span></li>");
                    }
                    else
                    {
                        string pageUrl = GetPageUrl(i);
                        pagination.Append($"<li class='page-item'><a class='page-link' href='{pageUrl}'>{i}</a></li>");
                    }
                }

                // Next button
                if (_currentPage < totalPages)
                {
                    string nextUrl = GetPageUrl(_currentPage + 1);
                    pagination.Append($"<li class='page-item'><a class='page-link' href='{nextUrl}'>Next</a></li>");
                }
                else
                {
                    pagination.Append("<li class='page-item disabled'><span class='page-link'>Next</span></li>");
                }

                pagination.Append("</ul>");
                pagination.Append($"<div class='pagination-info mt-2 text-center'>");
                pagination.Append($"<small class='text-muted'>Showing page {_currentPage} of {totalPages} ({totalTasks} total tasks)</small>");
                pagination.Append("</div>");
                pagination.Append("</nav>");

                litPagination.Text = pagination.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error generating pagination: {ex.Message}");
                litPagination.Text = "";
            }
        }

        /// <summary>
        /// Generates URL for a specific page with current filters
        /// </summary>
        private string GetPageUrl(int page)
        {
            var queryParams = new List<string>();
            
            if (page > 1) queryParams.Add($"page={page}");
            if (_pageSize != 20) queryParams.Add($"pageSize={_pageSize}");
            if (!string.IsNullOrEmpty(_statusFilter)) queryParams.Add($"status={_statusFilter}");

            string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            return Request.Path + queryString;
        }

        /// <summary>
        /// Event handler for GridView row data binding
        /// </summary>
        protected void gvTasks_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var task = e.Row.DataItem as AITask;
                if (task != null)
                {
                    // Find and populate the AI Model name
                    var litModelName = e.Row.FindControl("litModelName") as Literal;
                    if (litModelName != null)
                    {
                        try
                        {
                            var modelConfig = _aiModelConfigService.GetModelById(task.AIModelConfigurationId);
                            litModelName.Text = modelConfig?.DisplayName ?? "Unknown Model";
                        }
                        catch
                        {
                            litModelName.Text = "Unknown Model";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for status filter change
        /// </summary>
        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newStatus = ddlStatusFilter.SelectedValue;
            string redirectUrl = GetPageUrl(1); // Reset to page 1 when filtering
            
            if (!string.IsNullOrEmpty(newStatus))
            {
                redirectUrl += (redirectUrl.Contains("?") ? "&" : "?") + $"status={newStatus}";
            }
            
            Response.Redirect(redirectUrl);
        }

        /// <summary>
        /// Event handler for page size change
        /// </summary>
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int newPageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            string redirectUrl = Request.Path + $"?pageSize={newPageSize}";
            
            if (!string.IsNullOrEmpty(_statusFilter))
            {
                redirectUrl += $"&status={_statusFilter}";
            }
            
            Response.Redirect(redirectUrl);
        }

        /// <summary>
        /// Event handler for refresh button
        /// </summary>
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTaskData();
            LoadTaskStatistics();
        }

        /// <summary>
        /// Helper method to get prompt preview text
        /// </summary>
        protected string GetPromptPreview(string promptText)
        {
            if (string.IsNullOrEmpty(promptText))
                return "No prompt text";

            return promptText.Length > 100 ? promptText.Substring(0, 100) + "..." : promptText;
        }

        /// <summary>
        /// Helper method to get CSS class for task status
        /// </summary>
        protected string GetStatusCssClass(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "task-status";

            return $"task-status status-{status.ToLower()}";
        }

        /// <summary>
        /// Helper method to determine if there are active tasks (for auto-refresh)
        /// </summary>
        protected bool HasActiveTasks()
        {
            try
            {
                if (Session["UserId"] == null) return false;

                int userId = Convert.ToInt32(Session["UserId"]);
                var stats = _aiTaskService.GetUserTaskStatistics(userId);
                
                return stats.PendingTasks > 0 || stats.QueuedTasks > 0 || stats.ProcessingTasks > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
