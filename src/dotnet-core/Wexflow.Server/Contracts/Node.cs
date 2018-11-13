using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wexflow.Server.Contracts
{
    [DataContract]
    public class Node
    {
        [DataMember]
        public string Id { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string ParentId { get; private set; }

        public Node(string id, string name, string parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }
    }
}
