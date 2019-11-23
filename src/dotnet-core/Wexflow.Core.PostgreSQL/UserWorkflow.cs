namespace Wexflow.Core.PostgreSQL
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        public static readonly string TableStruct = "(ID SERIAL PRIMARY KEY, USER_ID CHAR(128), WORKFLOW_ID CHAR(128))";

        public string Id { get; set; }
    }
}
