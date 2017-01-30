using System.Collections.Generic;
using System.Linq;

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
            If = ifId;
            if (doNodes != null) DoNodes = doNodes.ToArray();
            if (otherwiseNodes != null) OtherwiseNodes = otherwiseNodes.ToArray();
        }
    }
}
