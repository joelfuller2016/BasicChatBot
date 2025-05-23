using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Data Access Layer for User entity
    /// Provides CRUD operations for user accounts
    /// </summary>
    public class UserDAL
    {
        /// <summary>
        /// Retrieves a user by their unique ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        public User GetUserById(int id)
        {
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, GoogleId, 
                       RegistrationDate, LastLoginDate, IsAdmin 
                FROM Users 
                WHERE Id = @Id";

            User result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToUser(reader);
            }, SQLiteHelper.CreateParameter("@Id", id));

            return result;
        }

        /// <summary>
        /// Retrieves a user by their username (case-insensitive)
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when username is null or empty</exception>
        public User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, GoogleId, 
                       RegistrationDate, LastLoginDate, IsAdmin 
                FROM Users 
                WHERE Username = @Username COLLATE NOCASE";

            User result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToUser(reader);
            }, SQLiteHelper.CreateParameter("@Username", username));

            return result;
        }

        /// <summary>
        /// Retrieves a user by their email address (case-insensitive)
        /// </summary>
        /// <param name="email">Email address to search for</param>
        /// <returns>User if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, GoogleId, 
                       RegistrationDate, LastLoginDate, IsAdmin 
                FROM Users 
                WHERE Email = @Email COLLATE NOCASE";

            User result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToUser(reader);
            }, SQLiteHelper.CreateParameter("@Email", email));

            return result;
        }

        /// <summary>
        /// Retrieves a user by their Google OAuth ID
        /// </summary>
        /// <param name="googleId">Google ID to search for</param>
        /// <returns>User if found, null otherwise</returns>
        /// <exception cref="ArgumentException">Thrown when googleId is null or empty</exception>
        public User GetUserByGoogleId(string googleId)
        {
            if (string.IsNullOrWhiteSpace(googleId))
                throw new ArgumentException("Google ID cannot be null or empty", nameof(googleId));

            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, GoogleId, 
                       RegistrationDate, LastLoginDate, IsAdmin 
                FROM Users 
                WHERE GoogleId = @GoogleId";

            User result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToUser(reader);
            }, SQLiteHelper.CreateParameter("@GoogleId", googleId));

            return result;
        }

        /// <summary>
        /// Creates a new user and returns the generated ID
        /// </summary>
        /// <param name="user">User object to insert (PasswordHash should be pre-set if applicable)</param>
        /// <returns>The ID of the newly created user</returns>
        /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public int CreateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required", nameof(user));

            const string insertSql = @"
                INSERT INTO Users (Username, Email, PasswordHash, GoogleId, RegistrationDate, IsAdmin) 
                VALUES (@Username, @Email, @PasswordHash, @GoogleId, @RegistrationDate, @IsAdmin)";

            const string getIdSql = "SELECT last_insert_rowid()";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@Username", user.Username),
                SQLiteHelper.CreateParameter("@Email", user.Email),
                SQLiteHelper.CreateParameter("@PasswordHash", user.PasswordHash),
                SQLiteHelper.CreateParameter("@GoogleId", user.GoogleId),
                SQLiteHelper.CreateParameter("@RegistrationDate", user.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss")),
                SQLiteHelper.CreateParameter("@IsAdmin", user.IsAdmin ? 1 : 0)
            };

            // Execute insert
            int rowsAffected = SQLiteHelper.ExecuteNonQuery(insertSql, parameters);
            
            if (rowsAffected != 1)
                throw new InvalidOperationException("Failed to create user - no rows affected");

            // Get the generated ID
            object idResult = SQLiteHelper.ExecuteScalar(getIdSql);
            return Convert.ToInt32(idResult);
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User object with updated values</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
        /// <exception cref="ArgumentException">Thrown when ID or Username is invalid</exception>
        public bool UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.Id <= 0)
                throw new ArgumentException("Valid user ID is required for update", nameof(user));

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required", nameof(user));

            const string sql = @"
                UPDATE Users 
                SET Username = @Username,
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    GoogleId = @GoogleId,
                    LastLoginDate = @LastLoginDate,
                    IsAdmin = @IsAdmin
                WHERE Id = @Id";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@Username", user.Username),
                SQLiteHelper.CreateParameter("@Email", user.Email),
                SQLiteHelper.CreateParameter("@PasswordHash", user.PasswordHash),
                SQLiteHelper.CreateParameter("@GoogleId", user.GoogleId),
                SQLiteHelper.CreateParameter("@LastLoginDate", user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss")),
                SQLiteHelper.CreateParameter("@IsAdmin", user.IsAdmin ? 1 : 0),
                SQLiteHelper.CreateParameter("@Id", user.Id)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Retrieves all users in the system
        /// </summary>
        /// <returns>List of all users</returns>
        public List<User> GetAllUsers()
        {
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, GoogleId, 
                       RegistrationDate, LastLoginDate, IsAdmin 
                FROM Users 
                ORDER BY Username";

            var results = new List<User>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToUser(reader));
            });

            return results;
        }

        /// <summary>
        /// Updates a user's admin status
        /// </summary>
        /// <param name="userId">ID of the user to update</param>
        /// <param name="isAdmin">New admin status</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool UpdateUserAdminStatus(int userId, bool isAdmin)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid user ID is required", nameof(userId));

            const string sql = @"
                UPDATE Users 
                SET IsAdmin = @IsAdmin 
                WHERE Id = @UserId";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@IsAdmin", isAdmin ? 1 : 0),
                SQLiteHelper.CreateParameter("@UserId", userId)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Updates a user's last login date
        /// </summary>
        /// <param name="userId">ID of the user to update</param>
        /// <param name="loginDate">Login date to set</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool UpdateLastLoginDate(int userId, DateTime loginDate)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid user ID is required", nameof(userId));

            const string sql = @"
                UPDATE Users 
                SET LastLoginDate = @LastLoginDate 
                WHERE Id = @UserId";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@LastLoginDate", loginDate.ToString("yyyy-MM-dd HH:mm:ss")),
                SQLiteHelper.CreateParameter("@UserId", userId)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Maps data from an SQLiteDataReader row to a User POCO
        /// </summary>
        /// <param name="reader">SQLiteDataReader positioned at a valid row</param>
        /// <returns>Populated User object</returns>
        private User MapReaderToUser(SQLiteDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                PasswordHash = reader["PasswordHash"]?.ToString(),
                GoogleId = reader["GoogleId"]?.ToString(),
                RegistrationDate = DateTime.Parse(reader["RegistrationDate"].ToString()),
                LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? 
                    DateTime.Parse(reader["LastLoginDate"].ToString()) : (DateTime?)null,
                IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
            };
        }
    }
}
