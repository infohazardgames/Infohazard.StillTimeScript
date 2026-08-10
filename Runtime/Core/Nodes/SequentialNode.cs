using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public abstract class SequentialNode : Node, ISequentialNode {
        public INode Next { get; set; }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return GetSingleNextNode(state);
        }

        public virtual INode GetSingleNextNode(StateContainer state) {
            return Next;
        }
    }
}
