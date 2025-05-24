using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Data Access Layer for AITask entity
    /// Provides CRUD operations for AI tasks
    /// </summary>
    public class AITaskDAL
    {
        /// <summary>
        /// Inserts a new AI task and returns the generated ID
        /// </summary>
        /// <param name="task">AITask to insert (CreatedAt will be set automatically)</param>
        /// <returns>The ID of the newly created task</returns>
        /// <exception cref="ArgumentNullException">Thrown when task is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public int Insert(AITask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.UserId <= 0)
                throw new ArgumentException("Valid UserId is required", nameof(task));

            if (task.AIModelConfigurationId <= 0)
                throw new ArgumentException("Valid AIModelConfigurationId is required", nameof(task));

            if (string.IsNullOrWhiteSpace(task.PromptText))
                throw new ArgumentException("PromptText is required", nameof(task));

            const string insertSql = @"
                INSERT INTO AITasks 
                (UserId, AIModelConfigurationId, TaskName, PromptText, Status, 
                 MaxTokens, Temperature, CreatedAt) 
                VALUES (@UserId, @AIModelConfigurationId, @TaskName, @PromptText, @Status,
                        @MaxTokens, @Temperature, @CreatedAt)";

            const string getIdSql = "SELECT last_insert_rowid()";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@UserId", task.UserId),
                SQLiteHelper.CreateParameter("@AIModelConfigurationId", task.AIModelConfigurationId),
                SQLiteHelper.CreateParameter("@TaskName", task.TaskName),
                SQLiteHelper.CreateParameter("@PromptText", task.PromptText),
                SQLiteHelper.CreateParameter("@Status", task.Status ?? "Pending"),
                SQLiteHelper.CreateParameter("@MaxTokens", task.MaxTokens),
                SQLiteHelper.CreateParameter("@Temperature", task.Temperature),
                SQLiteHelper.CreateParameter("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            // Execute insert
            int rowsAffected = SQLiteHelper.ExecuteNonQuery(insertSql, parameters);
            
            if (rowsAffected != 1)
                throw new InvalidOperationException("Failed to create AI task - no rows affected");

            // Get the generated ID
            object idResult = SQLiteHelper.ExecuteScalar(getIdSql);
            return Convert.ToInt32(idResult);
        }

        /// <summary>
        /// Retrieves an AI task by its primary key
        /// </summary>
        /// <param name="id">The ID of the task</param>
        /// <returns>AITask if found, null otherwise</returns>
        public AITask GetById(int id)
        {
            const string sql = @"
                SELECT Id, UserId, AIModelConfigurationId, TaskName, PromptText, Status,
                       MaxTokens, Temperature, CreatedAt, QueuedAt, ProcessingStartedAt,
                       CompletedAt, ErrorMessage
                FROM AITasks
                WHERE Id = @Id";

            AITask result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToAITask(reader);
            }, SQLiteHelper.CreateParameter("@Id", id));

            return result;
        }

        /// <summary>
        /// Retrieves AI tasks for a specific user with pagination
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of tasks per page</param>
        /// <returns>List of AI tasks for the user</returns>
        public List<AITask> GetByUserId(int userId, int pageNumber = 1, int pageSize = 20)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid UserId is required", nameof(userId));

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 20;

            int offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                SELECT Id, UserId, AIModelConfigurationId, TaskName, PromptText, Status,
                       MaxTokens, Temperature, CreatedAt, QueuedAt, ProcessingStartedAt,
                       CompletedAt, ErrorMessage
                FROM AITasks
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var results = new List<AITask>();

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@UserId", userId),
                SQLiteHelper.CreateParameter("@PageSize", pageSize),
                SQLiteHelper.CreateParameter("@Offset", offset)
            };

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToAITask(reader));
            }, parameters);

            return results;
        }

        /// <summary>
        /// Updates the status of an AI task and related timestamp
        /// </summary>
        /// <param name="taskId">ID of the task to update</param>
        /// <param name="newStatus">New status value</param>
        /// <param name="eventTime">Time when the status change occurred</param>
        /// <param name="errorMessage">Optional error message for failed tasks</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool UpdateStatus(int taskId, string newStatus, DateTime eventTime, string errorMessage = null)
        {
            if (taskId <= 0)
                throw new ArgumentException("Valid task ID is required", nameof(taskId));

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be null or empty", nameof(newStatus));

            // Determine which timestamp to update based on status
            string timestampField = null;
            switch (newStatus.ToLower())
            {
                case "queued":
                    timestampField = "QueuedAt";
                    break;
                case "processing":
                    timestampField = "ProcessingStartedAt";
                    break;
                case "completed":
                case "failed":
                    timestampField = "CompletedAt";
                    break;
            }

            string sql = "UPDATE AITasks SET Status = @Status";
            var parameters = new List<SQLiteParameter>
            {
                SQLiteHelper.CreateParameter("@Status", newStatus)
            };

            if (!string.IsNullOrEmpty(timestampField))
            {
                sql += $", {timestampField} = @EventTime";
                parameters.Add(SQLiteHelper.CreateParameter("@EventTime", eventTime.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                sql += ", ErrorMessage = @ErrorMessage";
                parameters.Add(SQLiteHelper.CreateParameter("@ErrorMessage", errorMessage));
            }

            sql += " WHERE Id = @TaskId";
            parameters.Add(SQLiteHelper.CreateParameter("@TaskId", taskId));

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters.ToArray());
            return rowsAffected == 1;
        }

        /// <summary>
        /// Retrieves queued tasks ready for processing
        /// </summary>
        /// <param name="limit">Maximum number of tasks to retrieve</param>
        /// <returns>List of queued AI tasks</returns>
        public List<AITask> GetQueuedTasks(int limit = 10)
        {
            if (limit < 1)
                limit = 10;

            const string sql = @"
                SELECT Id, UserId, AIModelConfigurationId, TaskName, PromptText, Status,
                       MaxTokens, Temperature, CreatedAt, QueuedAt, ProcessingStartedAt,
                       CompletedAt, ErrorMessage
                FROM AITasks
                WHERE Status = 'Queued'
                ORDER BY QueuedAt ASC
                LIMIT @Limit";

            var results = new List<AITask>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToAITask(reader));
            }, SQLiteHelper.CreateParameter("@Limit", limit));

            return results;
        }

        /// <summary>
        /// Retrieves AI tasks for a specific user with filtering and pagination
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of tasks per page</param>
        /// <param name="statusFilter">Optional status filter</param>
        /// <returns>List of AI tasks for the user</returns>
        public List<AITask> GetByUserIdWithFilter(int userId, int pageNumber = 1, int pageSize = 20, string statusFilter = null)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid UserId is required", nameof(userId));

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 20;

            int offset = (pageNumber - 1) * pageSize;

            string sql = @"
                SELECT Id, UserId, AIModelConfigurationId, TaskName, PromptText, Status,
                       MaxTokens, Temperature, CreatedAt, QueuedAt, ProcessingStartedAt,
                       CompletedAt, ErrorMessage
                FROM AITasks
                WHERE UserId = @UserId";

            var parameters = new List<SQLiteParameter>
            {
                SQLiteHelper.CreateParameter("@UserId", userId)
            };

            if (!string.IsNullOrEmpty(statusFilter))
            {
                sql += " AND Status = @Status";
                parameters.Add(SQLiteHelper.CreateParameter("@Status", statusFilter));
            }

            sql += @"
                ORDER BY CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            parameters.Add(SQLiteHelper.CreateParameter("@PageSize", pageSize));
            parameters.Add(SQLiteHelper.CreateParameter("@Offset", offset));

            var results = new List<AITask>();

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                results.Add(MapReaderToAITask(reader));
            }, parameters.ToArray());

            return results;
        }

        /// <summary>
        /// Gets the count of tasks by status for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="status">Status to count (optional, null for all)</param>
        /// <returns>Count of matching tasks</returns>
        public int GetTaskCountByStatus(int userId, string status = null)
        {
            string sql = "SELECT COUNT(*) FROM AITasks WHERE UserId = @UserId";
            var parameters = new List<SQLiteParameter>
            {
                SQLiteHelper.CreateParameter("@UserId", userId)
            };

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND Status = @Status";
                parameters.Add(SQLiteHelper.CreateParameter("@Status", status));
            }

            object result = SQLiteHelper.ExecuteScalar(sql, parameters.ToArray());
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Gets comprehensive task statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>UserTaskStatistics object with detailed statistics</returns>
        public UserTaskStatistics GetUserTaskStatistics(int userId)
        {
            var stats = new UserTaskStatistics();

            if (userId <= 0)
                return stats;

            const string taskCountsSql = @"
                SELECT 
                    Status,
                    COUNT(*) as Count
                FROM AITasks 
                WHERE UserId = @UserId 
                GROUP BY Status";

            // Get task counts by status
            SQLiteHelper.ExecuteReader(taskCountsSql, reader =>
            {
                string status = reader["Status"]?.ToString();
                int count = Convert.ToInt32(reader["Count"]);

                switch (status?.ToLower())
                {
                    case "pending":
                        stats.PendingTasks = count;
                        break;
                    case "queued":
                        stats.QueuedTasks = count;
                        break;
                    case "processing":
                        stats.ProcessingTasks = count;
                        break;
                    case "completed":
                        stats.CompletedTasks = count;
                        break;
                    case "failed":
                        stats.FailedTasks = count;
                        break;
                }
                stats.TotalTasks += count;
            }, SQLiteHelper.CreateParameter("@UserId", userId));

            // Get token usage and date statistics
            const string tokenStatsSql = @"
                SELECT 
                    COALESCE(SUM(atr.TokensUsed_Total), 0) as TotalTokens,
                    MAX(at.CreatedAt) as LastTaskCreated,
                    MAX(CASE WHEN at.Status = 'Completed' THEN at.CompletedAt END) as LastTaskCompleted
                FROM AITasks at
                LEFT JOIN AITaskResults atr ON at.Id = atr.AITaskId
                WHERE at.UserId = @UserId";

            SQLiteHelper.ExecuteReader(tokenStatsSql, reader =>
            {
                stats.TotalTokensUsed = reader["TotalTokens"] != DBNull.Value ? 
                    Convert.ToInt64(reader["TotalTokens"]) : 0;

                if (reader["LastTaskCreated"] != DBNull.Value)
                {
                    stats.LastTaskCreated = DateTime.Parse(reader["LastTaskCreated"].ToString());
                }

                if (reader["LastTaskCompleted"] != DBNull.Value)
                {
                    stats.LastTaskCompleted = DateTime.Parse(reader["LastTaskCompleted"].ToString());
                }
            }, SQLiteHelper.CreateParameter("@UserId", userId));

            return stats;
        }

        /// <summary>
        /// Maps data from an SQLiteDataReader row to an AITask POCO
        /// </summary>
        /// <param name="reader">SQLiteDataReader positioned at a valid row</param>
        /// <returns>Populated AITask object</returns>
        private AITask MapReaderToAITask(SQLiteDataReader reader)
        {
            return new AITask
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                AIModelConfigurationId = Convert.ToInt32(reader["AIModelConfigurationId"]),
                TaskName = reader["TaskName"]?.ToString(),
                PromptText = reader["PromptText"]?.ToString(),
                Status = reader["Status"]?.ToString(),
                MaxTokens = reader["MaxTokens"] != DBNull.Value ? 
                    Convert.ToInt32(reader["MaxTokens"]) : (int?)null,
                Temperature = reader["Temperature"] != DBNull.Value ? 
                    Convert.ToDouble(reader["Temperature"]) : (double?)null,
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                QueuedAt = reader["QueuedAt"] != DBNull.Value ? 
                    DateTime.Parse(reader["QueuedAt"].ToString()) : (DateTime?)null,
                ProcessingStartedAt = reader["ProcessingStartedAt"] != DBNull.Value ? 
                    DateTime.Parse(reader["ProcessingStartedAt"].ToString()) : (DateTime?)null,
                CompletedAt = reader["CompletedAt"] != DBNull.Value ? 
                    DateTime.Parse(reader["CompletedAt"].ToString()) : (DateTime?)null,
                ErrorMessage = reader["ErrorMessage"]?.ToString()
            };
        }
    }
}
