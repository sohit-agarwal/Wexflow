using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    public enum UserProfile
    {
        Administrator,
        Restricted
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public UserProfile UserProfile { get; set; }
    }
}
