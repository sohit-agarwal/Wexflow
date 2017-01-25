using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wexflow.Core
{
    public class Graph
    {
        public Node[] Nodes { get; private set; }
        public GraphEvent OnSuccess { get; private set; }
        public GraphEvent OnWarning { get; private set; }
        public GraphEvent OnError { get; private set; }

        public Graph(IEnumerable<Node> nodes,
            GraphEvent onSuccess, 
            GraphEvent onWarning, 
            GraphEvent onError)
        {
            if(nodes != null) this.Nodes = nodes.ToArray();
            this.OnSuccess = onSuccess;
            this.OnWarning = onWarning;
            this.OnError = onError;
        }
    }
}
