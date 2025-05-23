using System;
using System.Data;
using System.Data.SQLite;

namespace CSharpAIAssistant.DataAccess
{
    /// <summary>
    /// Static helper class for SQLite database operations
    /// Provides parameterized, secure methods for database access
    /// </summary>
    public static class SQLiteHelper
    {
        /// <summary>
        /// Gets the connection string for the SQLite database
        /// </summary>
        /// <returns>Connection string with foreign keys enabled</returns>
        private static string GetConnectionString()
        {
            if (string.IsNullOrEmpty(DbConfiguration.DatabasePath))
                throw new InvalidOperationException("Database path is not configured. Ensure DbConfiguration.DatabasePath is set.");
            
            return $"Data Source={DbConfiguration.DatabasePath};Version=3;Foreign Keys=True;";
        }

        /// <summary>
        /// Executes a non-query SQL statement (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sql">SQL statement to execute</param>
        /// <param name="parameters">Optional parameters for the SQL statement</param>
        /// <returns>Number of rows affected</returns>
        /// <exception cref="ArgumentException">Thrown when SQL is null or empty</exception>
        /// <exception cref="SQLiteException">Thrown when SQL execution fails</exception>
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be null or empty", nameof(sql));

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException($"Failed to execute non-query SQL: {sql}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL statement and returns a single scalar value
        /// </summary>
        /// <param name="sql">SQL statement to execute</param>
        /// <param name="parameters">Optional parameters for the SQL statement</param>
        /// <returns>Scalar value result or null</returns>
        /// <exception cref="ArgumentException">Thrown when SQL is null or empty</exception>
        /// <exception cref="SQLiteException">Thrown when SQL execution fails</exception>
        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be null or empty", nameof(sql));

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException($"Failed to execute scalar SQL: {sql}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL query and returns results as a DataTable
        /// </summary>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <returns>DataTable containing query results</returns>
        /// <exception cref="ArgumentException">Thrown when SQL is null or empty</exception>
        /// <exception cref="SQLiteException">Thrown when SQL execution fails</exception>
        public static DataTable GetDataTable(string sql, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be null or empty", nameof(sql));

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException($"Failed to execute query SQL: {sql}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL query and processes each row using a callback function
        /// </summary>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="rowProcessorCallback">Callback function to process each row</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <exception cref="ArgumentException">Thrown when SQL is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when rowProcessorCallback is null</exception>
        /// <exception cref="SQLiteException">Thrown when SQL execution fails</exception>
        public static void ExecuteReader(string sql, Action<SQLiteDataReader> rowProcessorCallback, params SQLiteParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL statement cannot be null or empty", nameof(sql));
            
            if (rowProcessorCallback == null)
                throw new ArgumentNullException(nameof(rowProcessorCallback));

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rowProcessorCallback(reader);
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException($"Failed to execute reader SQL: {sql}", ex);
            }
        }

        /// <summary>
        /// Creates a SQLiteParameter with proper null value handling
        /// </summary>
        /// <param name="name">Parameter name (should include @ prefix)</param>
        /// <param name="value">Parameter value (null values are converted to DBNull.Value)</param>
        /// <param name="dbType">Optional database type specification</param>
        /// <returns>Configured SQLiteParameter</returns>
        public static SQLiteParameter CreateParameter(string name, object value, DbType? dbType = null)
        {
            var parameter = new SQLiteParameter(name, value ?? DBNull.Value);
            
            if (dbType.HasValue)
            {
                parameter.DbType = dbType.Value;
            }
            
            return parameter;
        }

        /// <summary>
        /// Executes multiple SQL statements within a single transaction
        /// </summary>
        /// <param name="sqlStatements">Array of SQL statements to execute</param>
        /// <returns>Total number of rows affected by all statements</returns>
        /// <exception cref="ArgumentException">Thrown when sqlStatements is null or empty</exception>
        /// <exception cref="SQLiteException">Thrown when any SQL execution fails</exception>
        public static int ExecuteTransaction(params string[] sqlStatements)
        {
            if (sqlStatements == null || sqlStatements.Length == 0)
                throw new ArgumentException("SQL statements array cannot be null or empty", nameof(sqlStatements));

            int totalRowsAffected = 0;

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (string sql in sqlStatements)
                            {
                                if (!string.IsNullOrWhiteSpace(sql))
                                {
                                    using (var command = new SQLiteCommand(sql, connection, transaction))
                                    {
                                        totalRowsAffected += command.ExecuteNonQuery();
                                    }
                                }
                            }
                            
                            transaction.Commit();
                            return totalRowsAffected;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException("Failed to execute transaction", ex);
            }
        }
    }
}
