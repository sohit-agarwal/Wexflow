using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core
{
    public class GraphEvent
    {
        public Node[] DoNodes { get; private set; }

        public GraphEvent(IEnumerable<Node> doNodes)
        {
            if (doNodes != null) DoNodes = doNodes.ToArray();
        }
    }
}
