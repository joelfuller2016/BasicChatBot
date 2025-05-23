using System;
using System.Data.SQLite;
using CSharpAIAssistant.Models;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Data Access Layer for AITaskResult entity
    /// Provides CRUD operations for AI task results
    /// </summary>
    public class AITaskResultDAL
    {
        /// <summary>
        /// Inserts a new AI task result
        /// </summary>
        /// <param name="result">AITaskResult to insert (CreatedAt will be set automatically)</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
        /// <exception cref="ArgumentException">Thrown when required fields are missing</exception>
        public bool Insert(AITaskResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.AITaskId <= 0)
                throw new ArgumentException("Valid AITaskId is required", nameof(result));

            const string sql = @"
                INSERT INTO AITaskResults 
                (AITaskId, GeneratedContent, TokensUsed_Prompt, TokensUsed_Completion, 
                 TokensUsed_Total, ProcessingTimeMs, ModelUsedIdentifier, Success, CreatedAt) 
                VALUES (@AITaskId, @GeneratedContent, @TokensUsed_Prompt, @TokensUsed_Completion,
                        @TokensUsed_Total, @ProcessingTimeMs, @ModelUsedIdentifier, @Success, @CreatedAt)";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@AITaskId", result.AITaskId),
                SQLiteHelper.CreateParameter("@GeneratedContent", result.GeneratedContent),
                SQLiteHelper.CreateParameter("@TokensUsed_Prompt", result.TokensUsed_Prompt),
                SQLiteHelper.CreateParameter("@TokensUsed_Completion", result.TokensUsed_Completion),
                SQLiteHelper.CreateParameter("@TokensUsed_Total", result.TokensUsed_Total),
                SQLiteHelper.CreateParameter("@ProcessingTimeMs", result.ProcessingTimeMs),
                SQLiteHelper.CreateParameter("@ModelUsedIdentifier", result.ModelUsedIdentifier),
                SQLiteHelper.CreateParameter("@Success", result.Success ? 1 : 0),
                SQLiteHelper.CreateParameter("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Retrieves an AI task result by the associated task ID
        /// </summary>
        /// <param name="aiTaskId">The ID of the AI task</param>
        /// <returns>AITaskResult if found, null otherwise</returns>
        public AITaskResult GetByAITaskId(int aiTaskId)
        {
            if (aiTaskId <= 0)
                throw new ArgumentException("Valid AI task ID is required", nameof(aiTaskId));

            const string sql = @"
                SELECT Id, AITaskId, GeneratedContent, TokensUsed_Prompt, TokensUsed_Completion,
                       TokensUsed_Total, ProcessingTimeMs, ModelUsedIdentifier, Success, CreatedAt
                FROM AITaskResults
                WHERE AITaskId = @AITaskId";

            AITaskResult result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToAITaskResult(reader);
            }, SQLiteHelper.CreateParameter("@AITaskId", aiTaskId));

            return result;
        }

        /// <summary>
        /// Retrieves an AI task result by its primary key
        /// </summary>
        /// <param name="id">The ID of the task result</param>
        /// <returns>AITaskResult if found, null otherwise</returns>
        public AITaskResult GetById(int id)
        {
            const string sql = @"
                SELECT Id, AITaskId, GeneratedContent, TokensUsed_Prompt, TokensUsed_Completion,
                       TokensUsed_Total, ProcessingTimeMs, ModelUsedIdentifier, Success, CreatedAt
                FROM AITaskResults
                WHERE Id = @Id";

            AITaskResult result = null;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                result = MapReaderToAITaskResult(reader);
            }, SQLiteHelper.CreateParameter("@Id", id));

            return result;
        }

        /// <summary>
        /// Updates an existing AI task result
        /// </summary>
        /// <param name="result">AITaskResult with updated values</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        public bool Update(AITaskResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Id <= 0)
                throw new ArgumentException("Valid ID is required for update", nameof(result));

            const string sql = @"
                UPDATE AITaskResults 
                SET GeneratedContent = @GeneratedContent,
                    TokensUsed_Prompt = @TokensUsed_Prompt,
                    TokensUsed_Completion = @TokensUsed_Completion,
                    TokensUsed_Total = @TokensUsed_Total,
                    ProcessingTimeMs = @ProcessingTimeMs,
                    ModelUsedIdentifier = @ModelUsedIdentifier,
                    Success = @Success
                WHERE Id = @Id";

            var parameters = new[]
            {
                SQLiteHelper.CreateParameter("@GeneratedContent", result.GeneratedContent),
                SQLiteHelper.CreateParameter("@TokensUsed_Prompt", result.TokensUsed_Prompt),
                SQLiteHelper.CreateParameter("@TokensUsed_Completion", result.TokensUsed_Completion),
                SQLiteHelper.CreateParameter("@TokensUsed_Total", result.TokensUsed_Total),
                SQLiteHelper.CreateParameter("@ProcessingTimeMs", result.ProcessingTimeMs),
                SQLiteHelper.CreateParameter("@ModelUsedIdentifier", result.ModelUsedIdentifier),
                SQLiteHelper.CreateParameter("@Success", result.Success ? 1 : 0),
                SQLiteHelper.CreateParameter("@Id", result.Id)
            };

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected == 1;
        }

        /// <summary>
        /// Deletes an AI task result by ID
        /// </summary>
        /// <param name="id">ID of the result to delete</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool Delete(int id)
        {
            const string sql = "DELETE FROM AITaskResults WHERE Id = @Id";

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, 
                SQLiteHelper.CreateParameter("@Id", id));
            
            return rowsAffected == 1;
        }

        /// <summary>
        /// Deletes an AI task result by associated task ID
        /// </summary>
        /// <param name="aiTaskId">ID of the associated AI task</param>
        /// <returns>True if one row was affected, false otherwise</returns>
        public bool DeleteByAITaskId(int aiTaskId)
        {
            const string sql = "DELETE FROM AITaskResults WHERE AITaskId = @AITaskId";

            int rowsAffected = SQLiteHelper.ExecuteNonQuery(sql, 
                SQLiteHelper.CreateParameter("@AITaskId", aiTaskId));
            
            return rowsAffected == 1;
        }

        /// <summary>
        /// Checks if a result exists for the specified AI task
        /// </summary>
        /// <param name="aiTaskId">The ID of the AI task</param>
        /// <returns>True if a result exists, false otherwise</returns>
        public bool ExistsForTask(int aiTaskId)
        {
            const string sql = "SELECT COUNT(*) FROM AITaskResults WHERE AITaskId = @AITaskId";

            object result = SQLiteHelper.ExecuteScalar(sql, 
                SQLiteHelper.CreateParameter("@AITaskId", aiTaskId));
            
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Gets aggregate statistics for task results by user
        /// </summary>
        /// <param name="userId">User ID to get statistics for</param>
        /// <returns>Tuple containing (TotalTasks, SuccessfulTasks, TotalTokensUsed)</returns>
        public (int TotalTasks, int SuccessfulTasks, long TotalTokensUsed) GetUserStatistics(int userId)
        {
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalTasks,
                    SUM(CASE WHEN r.Success = 1 THEN 1 ELSE 0 END) as SuccessfulTasks,
                    SUM(COALESCE(r.TokensUsed_Total, 0)) as TotalTokensUsed
                FROM AITaskResults r
                INNER JOIN AITasks t ON r.AITaskId = t.Id
                WHERE t.UserId = @UserId";

            int totalTasks = 0;
            int successfulTasks = 0;
            long totalTokensUsed = 0;

            SQLiteHelper.ExecuteReader(sql, reader =>
            {
                totalTasks = Convert.ToInt32(reader["TotalTasks"]);
                successfulTasks = Convert.ToInt32(reader["SuccessfulTasks"]);
                totalTokensUsed = Convert.ToInt64(reader["TotalTokensUsed"]);
            }, SQLiteHelper.CreateParameter("@UserId", userId));

            return (totalTasks, successfulTasks, totalTokensUsed);
        }

        /// <summary>
        /// Maps data from an SQLiteDataReader row to an AITaskResult POCO
        /// </summary>
        /// <param name="reader">SQLiteDataReader positioned at a valid row</param>
        /// <returns>Populated AITaskResult object</returns>
        private AITaskResult MapReaderToAITaskResult(SQLiteDataReader reader)
        {
            return new AITaskResult
            {
                Id = Convert.ToInt32(reader["Id"]),
                AITaskId = Convert.ToInt32(reader["AITaskId"]),
                GeneratedContent = reader["GeneratedContent"]?.ToString(),
                TokensUsed_Prompt = reader["TokensUsed_Prompt"] != DBNull.Value ? 
                    Convert.ToInt32(reader["TokensUsed_Prompt"]) : (int?)null,
                TokensUsed_Completion = reader["TokensUsed_Completion"] != DBNull.Value ? 
                    Convert.ToInt32(reader["TokensUsed_Completion"]) : (int?)null,
                TokensUsed_Total = reader["TokensUsed_Total"] != DBNull.Value ? 
                    Convert.ToInt32(reader["TokensUsed_Total"]) : (int?)null,
                ProcessingTimeMs = reader["ProcessingTimeMs"] != DBNull.Value ? 
                    Convert.ToInt64(reader["ProcessingTimeMs"]) : (long?)null,
                ModelUsedIdentifier = reader["ModelUsedIdentifier"]?.ToString(),
                Success = Convert.ToBoolean(reader["Success"]),
                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString())
            };
        }
    }
}
