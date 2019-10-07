using LiteDB;

namespace Wexflow.Core.LiteDB
{
    public class Workflow : Core.Db.Workflow
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
