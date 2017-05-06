using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    public class Switch : Node
    {
        public int SwitchId { get; }
        public Case[] Cases { get; }
        public Node[] Default { get; }

        public Switch(int id, int parentId, int switchId, IEnumerable<Case> cases, IEnumerable<Node> @default) : base(id, parentId)
        {
            SwitchId = switchId;
            if (cases != null) Cases = cases.ToArray();
            if (@default != null) Default = @default.ToArray();
        }
    }
}
