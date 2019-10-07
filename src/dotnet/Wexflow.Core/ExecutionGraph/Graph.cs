using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph
{
    /// <summary>
    /// Execution graph.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Nodes.
        /// </summary>
        public Node[] Nodes { get; private set; }
        /// <summary>
        /// OnSuccess event.
        /// </summary>
        public GraphEvent OnSuccess { get; private set; }
        /// <summary>
        /// OnWarning event.
        /// </summary>
        public GraphEvent OnWarning { get; private set; }
        /// <summary>
        /// OnError event.
        /// </summary>
        public GraphEvent OnError { get; private set; }
        /// <summary>
        /// OnDisapproved event.
        /// </summary>
        public GraphEvent OnDisapproved { get; private set; }

        /// <summary>
        /// Creates a new execution graph.
        /// </summary>
        /// <param name="nodes">Nodes.</param>
        /// <param name="onSuccess">OnSuccess event.</param>
        /// <param name="onWarning">OnWarning event.</param>
        /// <param name="onError">OnError event.</param>
        /// <param name="onDisapproved">OnDisapproved event.</param>
        public Graph(IEnumerable<Node> nodes
            , GraphEvent onSuccess 
            , GraphEvent onWarning
            , GraphEvent onError
            , GraphEvent onDisapproved
            )
        {
            if(nodes != null) Nodes = nodes.ToArray();
            OnSuccess = onSuccess;
            OnWarning = onWarning;
            OnError = onError;
            OnDisapproved = onDisapproved;
        }
    }
}
