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
        public int Id { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public UserProfile UserProfile { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public double CreatedOn { get; set; }
        [DataMember]
        public double ModifiedOn { get; set; }
    }
}
