using LiteDB;

namespace Wexflow.Core.Db
{
    public class Workflow
    {
        [BsonId]
        public int Id { get; set; }
        public string Xml { get; set; }
    }
}
