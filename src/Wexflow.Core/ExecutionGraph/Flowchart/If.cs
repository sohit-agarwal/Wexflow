using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    public class If : Node
    {
        public int IfId { get; }
        public Node[] DoNodes { get; }
        public Node[] ElseNodes { get; }

        public If(int id, int parentId, int ifId, IEnumerable<Node> doNodes, IEnumerable<Node> elseNodes)
            :base(id, parentId)
        {
            IfId = ifId;
            if (doNodes != null) DoNodes = doNodes.ToArray();
            if (elseNodes != null) ElseNodes = elseNodes.ToArray();
        }
    }
}
