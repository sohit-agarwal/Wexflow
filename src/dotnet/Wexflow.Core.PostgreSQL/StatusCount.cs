namespace Wexflow.Core.PostgreSQL
{
    public class StatusCount : Core.Db.StatusCount
    {
        public static readonly string TableStruct = "(ID SERIAL PRIMARY KEY, PENDING_COUNT INT, RUNNING_COUNT INT, DONE_COUNT INT, FAILED_COUNT INT, WARNING_COUNT INT, DISABLED_COUNT INT, STOPPED_COUNT INT, DISAPPROVED_COUNT INT)";

        public int Id { get; set; }
    }
}