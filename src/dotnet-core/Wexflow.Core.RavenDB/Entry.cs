namespace Wexflow.Core.RavenDB
{
    public class Entry : Core.Db.Entry
    {
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
