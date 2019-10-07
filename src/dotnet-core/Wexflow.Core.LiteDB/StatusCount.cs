using LiteDB;

namespace Wexflow.Core.LiteDB
{
    public class StatusCount : Core.Db.StatusCount
    {
        [BsonId]
        public int Id { get; set; }
    }
}