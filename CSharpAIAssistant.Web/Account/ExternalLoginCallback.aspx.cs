using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Account
{
    public partial class ExternalLoginCallback : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                ProcessExternalLogin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error in ExternalLoginCallback: {0}", ex.ToString());
                Response.Redirect("~/Account/Login.aspx?message=googleerror");
            }
        }

        private void ProcessExternalLogin()
        {
            var loginInfo = Context.GetOwinContext().Authentication.GetExternalLoginInfo();
            
            if (loginInfo == null)
            {
                System.Diagnostics.Trace.TraceWarning("External login info is null - redirecting to login page");
                Response.Redirect("~/Account/Login.aspx?message=googleerror");
                return;
            }

            System.Diagnostics.Trace.WriteLine($"Processing external login: Provider={loginInfo.Login.LoginProvider}, ProviderKey={loginInfo.Login.ProviderKey}");

            // Extract information from external login
            string googleProviderKey = loginInfo.Login.ProviderKey;
            string email = loginInfo.Email;
            string name = loginInfo.DefaultUserName ?? loginInfo.ExternalIdentity.Name;

            // Extract additional claims if available
            var claims = loginInfo.ExternalIdentity.Claims.ToList();
            var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? email;
            var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? name;

            if (string.IsNullOrEmpty(googleProviderKey))
            {
                System.Diagnostics.Trace.TraceError("Google provider key is null or empty");
                Response.Redirect("~/Account/Login.aspx?message=googleerror");
                return;
            }

            var userDAL = new UserDAL();
            User user = null;

            // Try to find user by Google ID first
            user = userDAL.GetUserByGoogleId(googleProviderKey);

            if (user == null && !string.IsNullOrEmpty(emailClaim))
            {
                // Try to find user by email
                user = userDAL.GetUserByEmail(emailClaim);
                
                if (user != null)
                {
                    // User exists with this email but no Google ID - link the accounts
                    user.GoogleId = googleProviderKey;
                    userDAL.UpdateUser(user);
                    System.Diagnostics.Trace.WriteLine($"Linked existing user account with Google: UserID={user.Id}, Email={emailClaim}");
                }
            }

            if (user == null)
            {
                // Create new user account
                user = CreateNewGoogleUser(googleProviderKey, emailClaim, nameClaim, userDAL);
            }

            if (user != null)
            {
                // Login successful - create authentication session
                LoginUser(user);

                // Update last login date
                userDAL.UpdateLastLoginDate(user.Id, DateTime.UtcNow);

                System.Diagnostics.Trace.WriteLine($"Google OAuth login successful: UserID={user.Id}, Username={user.Username}");

                // Sign out of external cookie
                Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                // Redirect to default page
                Response.Redirect("~/Default.aspx");
            }
            else
            {
                System.Diagnostics.Trace.TraceError("Failed to create or retrieve user during Google OAuth");
                Response.Redirect("~/Account/Login.aspx?message=googleerror");
            }
        }

        private User CreateNewGoogleUser(string googleId, string email, string name, UserDAL userDAL)
        {
            try
            {
                // Generate a unique username based on name or email
                string baseUsername = GenerateUsernameFromNameOrEmail(name, email);
                string username = EnsureUniqueUsername(baseUsername, userDAL);

                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    GoogleId = googleId,
                    PasswordHash = null, // No password for Google OAuth users
                    RegistrationDate = DateTime.UtcNow,
                    IsAdmin = false
                };

                int userId = userDAL.CreateUser(newUser);
                
                if (userId > 0)
                {
                    newUser.Id = userId;
                    System.Diagnostics.Trace.WriteLine($"Created new Google user: ID={userId}, Username={username}, Email={email}");
                    return newUser;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error creating new Google user: {0}", ex.ToString());
                return null;
            }
        }

        private string GenerateUsernameFromNameOrEmail(string name, string email)
        {
            // Try to use name first
            if (!string.IsNullOrEmpty(name))
            {
                // Clean up the name to make it username-friendly
                string cleanName = name.Replace(" ", "").Replace(".", "").Replace("-", "");
                if (cleanName.Length >= 3)
                {
                    return cleanName.Length > 20 ? cleanName.Substring(0, 20) : cleanName;
                }
            }

            // Fall back to email prefix
            if (!string.IsNullOrEmpty(email) && email.Contains("@"))
            {
                string emailPrefix = email.Split('@')[0];
                return emailPrefix.Replace(".", "").Replace("-", "").Replace("_", "");
            }

            // Last resort
            return "googleuser" + DateTime.Now.Ticks.ToString().Substring(0, 6);
        }

        private string EnsureUniqueUsername(string baseUsername, UserDAL userDAL)
        {
            string username = baseUsername;
            int counter = 1;

            while (userDAL.GetUserByUsername(username) != null)
            {
                username = baseUsername + counter.ToString();
                counter++;
                
                if (counter > 100) // Safety valve
                {
                    username = baseUsername + DateTime.Now.Ticks.ToString().Substring(0, 4);
                    break;
                }
            }

            return username;
        }

        private void LoginUser(User user)
        {
            // Create Forms Authentication ticket
            FormsAuthentication.SetAuthCookie(user.Username, false);

            // Store user information in session
            Session["UserId"] = user.Id;
            Session["Username"] = user.Username;
            Session["IsAdmin"] = user.IsAdmin;
        }
    }
}
