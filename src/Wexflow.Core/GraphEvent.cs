using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class GraphEvent
    {
        public Node[] DoNodes { get; private set; }

        public GraphEvent(IEnumerable<Node> doNodes)
        {
            if (doNodes != null) this.DoNodes = doNodes.ToArray();
        }
    }
}
