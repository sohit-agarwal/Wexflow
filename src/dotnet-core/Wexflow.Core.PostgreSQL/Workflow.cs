namespace Wexflow.Core.PostgreSQL
{
    public class Workflow: Core.Db.Workflow
    {
        public static readonly string TableStruct = "(ID SERIAL PRIMARY KEY, XML XML)";

        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
