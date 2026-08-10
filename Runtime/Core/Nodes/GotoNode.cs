using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class GotoNode : Node, ISingleNextNode {
        public INode Target { get; }
        
        public GotoNode(INode target) {
            Target = target;
        }
        
        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return Target;
        }
        
        public INode GetSingleNextNode(StateContainer state) {
            return Target;
        }
    }
}