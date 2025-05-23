using System;

namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents a user account in the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username (unique, case-insensitive)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email address (unique, case-insensitive, optional)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password (optional for OAuth users)
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the Google OAuth identifier (optional)
        /// </summary>
        public string GoogleId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user registered
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the user's last login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets whether the user has administrative privileges
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Initializes a new instance of the User class
        /// </summary>
        public User()
        {
            RegistrationDate = DateTime.UtcNow;
            IsAdmin = false;
        }
    }
}
