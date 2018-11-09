using System;
using LiteDB;

namespace Wexflow.Core.Db
{
    public enum UserProfile
    {
        Administrator,
        Restricted
    }

    public class User
    {
        [BsonId]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public UserProfile UserProfile { get; set; }
        public string Email { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
