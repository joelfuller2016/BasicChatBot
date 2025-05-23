using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace CSharpAIAssistant.Web.Account
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Get current user info for logging
                string username = Session["Username"]?.ToString() ?? "Unknown";
                int? userId = Session["UserId"] as int?;

                // Sign out of Forms Authentication
                FormsAuthentication.SignOut();

                // Clear all session data
                Session.Clear();
                Session.Abandon();

                // Clear authentication cookies
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
                    {
                        Expires = DateTime.Now.AddYears(-1)
                    };
                    Response.Cookies.Add(cookie);
                }

                // Clear any OWIN authentication cookies
                Context.GetOwinContext().Authentication.SignOut();

                // Log the logout
                System.Diagnostics.Trace.WriteLine($"User logged out: ID={userId}, Username={username}");

                // The page will auto-redirect via JavaScript after showing the logout message
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error during logout: {0}", ex.ToString());
                
                // Even if there's an error, still redirect to login
                Response.Redirect("~/Account/Login.aspx?message=loggedout");
            }
        }
    }
}
