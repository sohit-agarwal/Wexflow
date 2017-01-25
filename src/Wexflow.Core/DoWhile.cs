using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class DoWhile: Node
    {
        public int While { get; private set; }
        public Node[] DoNodes { get; private set; }

        public DoWhile(int id, int parentId, int whileId, IEnumerable<Node> doNodes):base(id, parentId)
        {
            this.While = whileId;
            if (doNodes != null) this.DoNodes = doNodes.ToArray();
        }
    }
}
