using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class Node
    {
        public int Id { get; private set; }
        public int ParentId { get; private set; }

        public Node(int id, int parentId)
        {
            this.Id = id;
            this.ParentId = parentId;
        }
    }
}
