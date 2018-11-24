using LiteDB;

namespace Wexflow.Core.Db
{
    public class StatusCount
    {
        [BsonId]
        public int Id { get; set; }
        public int PendingCount { get; set; }
        public int RunningCount { get; set; }
        public int DoneCount { get; set; }
        public int FailedCount { get; set; }
        public int WarningCount { get; set; }
        public int DisabledCount { get; set; }
        public int StoppedCount { get; set; }
    }
}