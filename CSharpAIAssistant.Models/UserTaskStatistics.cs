using System;

namespace CSharpAIAssistant.Models
{
    /// <summary>
    /// Represents task statistics for a user
    /// </summary>
    public class UserTaskStatistics
    {
        /// <summary>
        /// Total number of tasks created by the user
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// Number of tasks with Completed status
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// Number of tasks with Pending status
        /// </summary>
        public int PendingTasks { get; set; }

        /// <summary>
        /// Number of tasks with Queued status
        /// </summary>
        public int QueuedTasks { get; set; }

        /// <summary>
        /// Number of tasks with Processing status
        /// </summary>
        public int ProcessingTasks { get; set; }

        /// <summary>
        /// Number of tasks with Failed status
        /// </summary>
        public int FailedTasks { get; set; }

        /// <summary>
        /// Total tokens used across all completed tasks
        /// </summary>
        public long TotalTokensUsed { get; set; }

        /// <summary>
        /// Average tokens used per completed task
        /// </summary>
        public double AverageTokensPerTask 
        { 
            get 
            { 
                return CompletedTasks > 0 ? (double)TotalTokensUsed / CompletedTasks : 0; 
            } 
        }

        /// <summary>
        /// Success rate as a percentage
        /// </summary>
        public double SuccessRate 
        { 
            get 
            { 
                return TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0; 
            } 
        }

        /// <summary>
        /// Number of active tasks (Pending + Queued + Processing)
        /// </summary>
        public int ActiveTasks 
        { 
            get 
            { 
                return PendingTasks + QueuedTasks + ProcessingTasks; 
            } 
        }

        /// <summary>
        /// Date of the most recent task creation
        /// </summary>
        public DateTime? LastTaskCreated { get; set; }

        /// <summary>
        /// Date of the most recent task completion
        /// </summary>
        public DateTime? LastTaskCompleted { get; set; }

        /// <summary>
        /// Default constructor initializing all counts to zero
        /// </summary>
        public UserTaskStatistics()
        {
            TotalTasks = 0;
            CompletedTasks = 0;
            PendingTasks = 0;
            QueuedTasks = 0;
            ProcessingTasks = 0;
            FailedTasks = 0;
            TotalTokensUsed = 0;
            LastTaskCreated = null;
            LastTaskCompleted = null;
        }
    }
}
