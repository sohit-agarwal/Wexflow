namespace Wexflow.Core.PostgreSQL
{
    public class Entry : Core.Db.Entry
    {
        public static readonly string TableStruct = "(ID SERIAL PRIMARY KEY, NAME CHAR(128), DESCRIPTION CHAR(128), LAUNCH_TYPE INT, STATUS_DATE TIMESTAMP, STATUS WORKFLOW_ID INT)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
