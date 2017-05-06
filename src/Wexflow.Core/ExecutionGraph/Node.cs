
namespace Wexflow.Core.ExecutionGraph
{
    public class Node
    {
        public int Id { get; }
        public int ParentId { get; }

        public Node(int id, int parentId)
        {
            Id = id;
            ParentId = parentId;
        }
    }
}
