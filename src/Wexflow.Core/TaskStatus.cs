
namespace Wexflow.Core
{
    public enum Status
    {
        Success,
        Warning,
        Error
    }

    public class TaskStatus
    {
        public Status Status { get; set; }
        /// <summary>
        /// DoIf and DoWhile condition
        /// </summary>
        public bool Condition { get; set; }

        public TaskStatus(Status status)
        {
            Status = status;
        }

        public TaskStatus(Status status, bool condition) : this(status)
        {
            Condition = condition;
        }
    }
}
