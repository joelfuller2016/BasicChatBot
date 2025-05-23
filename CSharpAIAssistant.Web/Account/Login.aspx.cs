using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Microsoft.Owin.Security;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.DataAccess;

namespace CSharpAIAssistant.Web.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is already authenticated, redirect if so
                if (User.Identity.IsAuthenticated)
                {
                    Response.Redirect("~/Default.aspx");
                }

                // Check if Google authentication is configured
                CheckGoogleAuthConfiguration();

                // Show any query string messages
                string message = Request.QueryString["message"];
                if (!string.IsNullOrEmpty(message))
                {
                    switch (message.ToLower())
                    {
                        case "registered":
                            ShowInfo("Registration successful! Please sign in with your new account.");
                            break;
                        case "loggedout":
                            ShowInfo("You have been signed out successfully.");
                            break;
                        case "unauthorized":
                            ShowError("You must be signed in to access that page.");
                            break;
                        case "googleerror":
                            ShowError("There was an error with Google sign-in. Please try again or use username/password.");
                            break;
                    }
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                // Clear previous messages
                pnlError.Visible = false;
                pnlInfo.Visible = false;

                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("Both username and password are required.");
                    return;
                }

                // Authenticate user
                var userDAL = new UserDAL();
                var user = userDAL.GetUserByUsername(username);

                if (user == null)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Authentication successful
                LoginUser(user.Id, user.Username, user.IsAdmin, chkRememberMe.Checked);

                // Update last login date
                userDAL.UpdateLastLoginDate(user.Id, DateTime.UtcNow);

                System.Diagnostics.Trace.WriteLine($"User logged in via forms auth: ID={user.Id}, Username={user.Username}");

                // Redirect to default page or return URL
                string returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("~/"))
                {
                    Response.Redirect(returnUrl);
                }
                else
                {
                    Response.Redirect("~/Default.aspx");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error during login: {0}", ex.ToString());
                ShowError("An error occurred during sign-in. Please try again.");
            }
        }

        protected void btnLoginWithGoogle_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if Google authentication is configured
                var settingsService = new ApplicationSettingsService();
                string googleClientId = settingsService.GetGoogleClientId();
                
                if (string.IsNullOrEmpty(googleClientId) || googleClientId.Contains("YOUR_GOOGLE_CLIENT_ID"))
                {
                    ShowError("Google sign-in is not configured. Please contact the administrator or use username/password login.");
                    return;
                }

                // Initiate Google OAuth challenge
                string returnUrl = ResolveUrl("~/Account/ExternalLoginCallback.aspx");
                var properties = new AuthenticationProperties { RedirectUri = returnUrl };
                
                Context.GetOwinContext().Authentication.Challenge(properties, "Google");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error initiating Google OAuth: {0}", ex.ToString());
                ShowError("Unable to initiate Google sign-in. Please try again or use username/password.");
            }
        }

        private void LoginUser(int userId, string username, bool isAdmin, bool rememberMe)
        {
            // Create authentication ticket
            FormsAuthentication.SetAuthCookie(username, rememberMe);

            // Store user information in session
            Session["UserId"] = userId;
            Session["Username"] = username;
            Session["IsAdmin"] = isAdmin;
        }

        private void CheckGoogleAuthConfiguration()
        {
            try
            {
                var settingsService = new ApplicationSettingsService();
                string googleClientId = settingsService.GetGoogleClientId();
                
                if (string.IsNullOrEmpty(googleClientId) || 
                    googleClientId.Contains("YOUR_GOOGLE_CLIENT_ID") ||
                    googleClientId == "YOUR_GOOGLE_CLIENT_ID_HERE")
                {
                    btnLoginWithGoogle.Visible = false;
                    pnlGoogleDisabled.Visible = true;
                }
                else
                {
                    btnLoginWithGoogle.Visible = true;
                    pnlGoogleDisabled.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error checking Google auth configuration: {0}", ex.Message);
                btnLoginWithGoogle.Visible = false;
                pnlGoogleDisabled.Visible = true;
            }
        }

        private void ShowError(string message)
        {
            litError.Text = message;
            pnlError.Visible = true;
            pnlInfo.Visible = false;
        }

        private void ShowInfo(string message)
        {
            litInfo.Text = message;
            pnlInfo.Visible = true;
            pnlError.Visible = false;
        }
    }
}
