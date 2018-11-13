using System;
using LiteDB;

namespace Wexflow.Core.Db
{
    public enum Status
    {
        Pending,
        Running,
        Done,
        Failed,
        Warning,
        Disabled,
        Stopped
    }

    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic,
        Cron
    }

    public class Entry
    {
        [BsonId]
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public LaunchType LaunchType { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public DateTime StatusDate { get; set; }
    }
}
