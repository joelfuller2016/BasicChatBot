using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Admin
{
    public partial class UserManagement : Page
    {
        private UserDAL _userDAL;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verify admin authorization
            if (Session["IsAdmin"] == null || !Convert.ToBoolean(Session["IsAdmin"]))
            {
                Response.Redirect("~/Account/Login.aspx?message=unauthorized");
                return;
            }

            _userDAL = new UserDAL();

            if (!IsPostBack)
            {
                BindUsersGrid();
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var user = e.Row.DataItem as User;
                if (user == null) return;

                // Find the admin checkbox
                CheckBox chkIsAdmin = e.Row.FindControl("chkIsAdmin") as CheckBox;
                if (chkIsAdmin != null)
                {
                    // Get current user ID from session
                    int currentUserId = Convert.ToInt32(Session["UserId"] ?? 0);

                    // Disable checkbox for current user to prevent self-demotion
                    if (user.Id == currentUserId)
                    {
                        chkIsAdmin.Enabled = false;
                        chkIsAdmin.ToolTip = "You cannot modify your own admin status";
                        chkIsAdmin.CssClass += " disabled";
                    }
                    else
                    {
                        // Check if this is the last admin (excluding current user)
                        var allUsers = _userDAL.GetAllUsers();
                        int adminCount = allUsers.Count(u => u.IsAdmin);
                        
                        if (user.IsAdmin && adminCount <= 1)
                        {
                            chkIsAdmin.Enabled = false;
                            chkIsAdmin.ToolTip = "Cannot remove the last administrator";
                            chkIsAdmin.CssClass += " disabled";
                        }
                        else
                        {
                            chkIsAdmin.ToolTip = user.IsAdmin ? 
                                "Click to remove admin privileges" : 
                                "Click to grant admin privileges";
                        }
                    }
                }
            }
        }

        protected void chkIsAdmin_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkIsAdmin = sender as CheckBox;
                if (chkIsAdmin == null) return;

                // Find the row and get the user ID
                GridViewRow row = chkIsAdmin.NamingContainer as GridViewRow;
                if (row == null) return;

                int userId = Convert.ToInt32(gvUsers.DataKeys[row.RowIndex].Value);
                bool newAdminStatus = chkIsAdmin.Checked;

                // Get current user ID for safety checks
                int currentUserId = Convert.ToInt32(Session["UserId"] ?? 0);

                // Prevent self-modification
                if (userId == currentUserId)
                {
                    ShowError("You cannot modify your own admin status.");
                    BindUsersGrid(); // Reset the grid
                    return;
                }

                // Get user details for logging
                var user = _userDAL.GetUserById(userId);
                if (user == null)
                {
                    ShowError("User not found.");
                    BindUsersGrid();
                    return;
                }

                // Prevent removing the last admin
                if (user.IsAdmin && !newAdminStatus)
                {
                    var allUsers = _userDAL.GetAllUsers();
                    int adminCount = allUsers.Count(u => u.IsAdmin);
                    
                    if (adminCount <= 1)
                    {
                        ShowError("Cannot remove the last administrator. At least one admin must remain.");
                        BindUsersGrid(); // Reset the grid
                        return;
                    }
                }

                // Update admin status
                bool success = _userDAL.UpdateUserAdminStatus(userId, newAdminStatus);

                if (success)
                {
                    string action = newAdminStatus ? "granted" : "removed";
                    ShowSuccess($"Admin privileges {action} for user '{user.Username}' successfully.");
                    
                    System.Diagnostics.Trace.WriteLine($"Admin status changed: User={user.Username} (ID={userId}), NewStatus={newAdminStatus}, ChangedBy={Session["Username"]} (ID={currentUserId})");
                    
                    BindUsersGrid(); // Refresh to update UI state
                }
                else
                {
                    ShowError("Failed to update user admin status. Please try again.");
                    BindUsersGrid(); // Reset the grid
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error updating user admin status: {0}", ex.ToString());
                ShowError("An error occurred while updating user privileges. Please try again.");
                BindUsersGrid(); // Reset the grid
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            BindUsersGrid();
            ShowSuccess("User list refreshed successfully.");
        }

        private void BindUsersGrid()
        {
            try
            {
                var users = _userDAL.GetAllUsers()
                    .OrderBy(u => u.Username)
                    .ToList();

                gvUsers.DataSource = users;
                gvUsers.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error binding users grid: {0}", ex.ToString());
                ShowError("Error loading users. Please refresh the page.");
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
