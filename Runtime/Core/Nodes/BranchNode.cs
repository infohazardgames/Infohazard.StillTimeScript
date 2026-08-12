using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class BranchNode : TextNode {
        public List<IBranchOption> Options { get; } = new();

        public BranchNode(IExpression textExpression, Speaker speaker) : base(textExpression, speaker) { }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            return Options.Where(o => o.IsAvailable(state)).Select(o => o.GetNextNode(state));
        }
    }
}
