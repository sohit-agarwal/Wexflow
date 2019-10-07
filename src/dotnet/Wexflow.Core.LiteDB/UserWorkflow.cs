using LiteDB;

namespace Wexflow.Core.LiteDB
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        [BsonId]
        public int Id { get; set; }
    }
}
