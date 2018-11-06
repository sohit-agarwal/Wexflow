using LiteDB;

namespace Wexflow.Core.Db
{
    public class User
    {
        [BsonId]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
