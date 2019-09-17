using LiteDB;

namespace Wexflow.Core.LiteDB
{
    public class Entry : Core.Db.Entry
    {
        [BsonId]
        public int Id { get; set; }

        public override string GetDbId()
        {
            return Id.ToString();
        }
    }
}
