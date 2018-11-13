using LiteDB;
using System;

namespace Wexflow.Core.Db
{
    public class HistoryEntry
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
