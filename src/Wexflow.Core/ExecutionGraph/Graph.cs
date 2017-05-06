using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph
{
    public class Graph
    {
        public Node[] Nodes { get; }
        public GraphEvent OnSuccess { get; }
        public GraphEvent OnWarning { get; }
        public GraphEvent OnError { get; }

        public Graph(IEnumerable<Node> nodes,
            GraphEvent onSuccess, 
            GraphEvent onWarning, 
            GraphEvent onError)
        {
            if(nodes != null) Nodes = nodes.ToArray();
            OnSuccess = onSuccess;
            OnWarning = onWarning;
            OnError = onError;
        }
    }
}
