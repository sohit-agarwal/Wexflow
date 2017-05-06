using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph
{
    public class GraphEvent
    {
        public Node[] DoNodes { get; }

        public GraphEvent(IEnumerable<Node> doNodes)
        {
            if (doNodes != null) DoNodes = doNodes.ToArray();
        }
    }
}
