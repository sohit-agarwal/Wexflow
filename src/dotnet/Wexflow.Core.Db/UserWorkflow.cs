using LiteDB;

namespace Wexflow.Core.Db
{
    public class UserWorkflow
    {
        [BsonId]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkflowId { get; set; }
    }
}
