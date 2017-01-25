using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class DoIf : Node
    {
        public int If { get; private set; }
        public Node[] DoNodes { get; private set; }
        public Node[] OtherwiseNodes { get; private set; }

        public DoIf(int id, int parentId, int ifId, IEnumerable<Node> doNodes, IEnumerable<Node> otherwiseNodes)
            :base(id, parentId)
        {
            this.If = ifId;
            if (doNodes != null) this.DoNodes = doNodes.ToArray();
            if (otherwiseNodes != null) this.OtherwiseNodes = otherwiseNodes.ToArray();
        }
    }
}
