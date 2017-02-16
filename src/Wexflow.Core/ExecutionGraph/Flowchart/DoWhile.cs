using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    public class DoWhile: Node
    {
        public int While { get; private set; }
        public Node[] DoNodes { get; private set; }

        public DoWhile(int id, int parentId, int whileId, IEnumerable<Node> doNodes):base(id, parentId)
        {
            While = whileId;
            if (doNodes != null) DoNodes = doNodes.ToArray();
        }
    }
}
