using System;
using System.Data.SQLite;
using System.Web.UI;
using CSharpAIAssistant.BusinessLogic;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.Web.Account
{
    public partial class Register : Page
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
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                // Clear previous messages
                pnlError.Visible = false;
                pnlSuccess.Visible = false;

                // Validate password length
                if (txtPassword.Text.Length < 8)
                {
                    ShowError("Password must be at least 8 characters long.");
                    return;
                }

                // Validate input
                string username = txtUsername.Text.Trim();
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ShowError("All fields are required.");
                    return;
                }

                // Check for existing username or email
                var userDAL = new UserDAL();
                
                if (userDAL.GetUserByUsername(username) != null)
                {
                    ShowError("Username is already taken. Please choose a different username.");
                    return;
                }

                if (userDAL.GetUserByEmail(email) != null)
                {
                    ShowError("An account with this email address already exists.");
                    return;
                }

                // Hash password
                string hashedPassword = PasswordHasher.HashPassword(password);

                // Create new user
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = hashedPassword,
                    RegistrationDate = DateTime.UtcNow,
                    IsAdmin = false
                };

                // Save user
                int newUserId = userDAL.CreateUser(newUser);

                if (newUserId > 0)
                {
                    // Registration successful
                    ShowSuccess("Account created successfully! You can now <a href=\"Login.aspx\">sign in</a>.");
                    
                    // Clear form
                    txtUsername.Text = "";
                    txtEmail.Text = "";
                    txtPassword.Text = "";
                    txtConfirmPassword.Text = "";

                    System.Diagnostics.Trace.WriteLine($"New user registered: ID={newUserId}, Username={username}, Email={email}");
                }
                else
                {
                    ShowError("Failed to create account. Please try again.");
                }
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Trace.TraceError("Database error during registration: {0}", ex.ToString());
                
                // Check for specific constraint violations
                if (ex.Message.Contains("UNIQUE constraint failed: Users.Username"))
                {
                    ShowError("Username is already taken. Please choose a different username.");
                }
                else if (ex.Message.Contains("UNIQUE constraint failed: Users.Email"))
                {
                    ShowError("An account with this email address already exists.");
                }
                else
                {
                    ShowError("A database error occurred. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error during registration: {0}", ex.ToString());
                ShowError("An unexpected error occurred. Please try again later.");
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
