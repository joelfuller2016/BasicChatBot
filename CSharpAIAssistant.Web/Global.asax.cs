using System;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Web;
using CSharpAIAssistant.DataAccess;
using CSharpAIAssistant.BusinessLogic;

namespace CSharpAIAssistant.Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            
            // Initialize database
            InitializeDatabase();
            
            // Start background task processor
            AITaskProcessor.Start();
            
            Trace.WriteLine("CSharpAIAssistant application started successfully.");
        }

        void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
            AITaskProcessor.Stop();
            
            Trace.WriteLine("CSharpAIAssistant application ended.");
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception ex = Server.GetLastError();
            Trace.TraceError("Unhandled application error: {0}", ex?.ToString());
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }

        private void InitializeDatabase()
        {
            try
            {
                // Get database file name from Web.config
                string dbFileName = ConfigurationManager.AppSettings["SQLiteDbFileName"];
                if (string.IsNullOrEmpty(dbFileName))
                {
                    dbFileName = "CSharpAIAssistant.db";
                    Trace.TraceWarning("SQLiteDbFileName not found in Web.config, using default: {0}", dbFileName);
                }

                // Ensure App_Data directory exists
                string appDataPath = Server.MapPath("~/App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                    Trace.WriteLine("Created App_Data directory: {0}", appDataPath);
                }

                // Construct full database path
                string dbPath = Path.Combine(appDataPath, dbFileName);
                DbConfiguration.DatabasePath = dbPath;

                Trace.WriteLine("Database path set to: {0}", dbPath);

                // Check if database file exists
                if (!File.Exists(dbPath))
                {
                    Trace.WriteLine("Database file does not exist. Creating new database.");
                    
                    // Create database file
                    SQLiteConnection.CreateFile(dbPath);
                    Trace.WriteLine("SQLite database file created: {0}", dbPath);

                    // Initialize schema and seed data
                    CreateSchemaAndSeedData(dbPath);
                }
                else
                {
                    Trace.WriteLine("Database file already exists: {0}", dbPath);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to initialize database: {0}", ex.ToString());
                throw new ApplicationException("Database initialization failed. See trace logs for details.", ex);
            }
        }

        private void CreateSchemaAndSeedData(string dbPath)
        {
            string connectionString = $"Data Source={dbPath};Version=3;Foreign Keys=True;";
            
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                Trace.WriteLine("Database connection opened for schema creation.");

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Execute all DDL statements
                        foreach (string ddlStatement in SqlSchemaConstants.AllDDLStatements)
                        {
                            using (var command = new SQLiteCommand(ddlStatement, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        Trace.WriteLine("Database schema created successfully. Executed {0} DDL statements.", 
                                      SqlSchemaConstants.AllDDLStatements.Length);

                        // Seed initial data
                        CSharpAIAssistant.BusinessLogic.DataSeeder.SeedInitialData(connection, transaction);

                        // Commit transaction
                        transaction.Commit();
                        Trace.WriteLine("Database schema creation and data seeding completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error during schema creation or data seeding: {0}", ex.ToString());
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    // Placeholder configuration classes - these would typically be in App_Start folder
    // Removed bundling and routing for simplicity - using basic Web Forms navigation
}
