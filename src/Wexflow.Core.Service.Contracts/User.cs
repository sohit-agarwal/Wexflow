using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
}
