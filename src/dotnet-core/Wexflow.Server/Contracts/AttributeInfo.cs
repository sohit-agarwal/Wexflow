using System.Runtime.Serialization;

namespace Wexflow.Server.Contracts
{
    [DataContract]
    public class AttributeInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }

        public AttributeInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
