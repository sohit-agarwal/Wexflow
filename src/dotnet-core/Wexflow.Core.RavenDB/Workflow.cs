namespace Wexflow.Core.RavenDB
{
    public class Workflow: Core.Db.Workflow
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
