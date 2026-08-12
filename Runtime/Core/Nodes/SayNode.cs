using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class SayNode : TextNode, ISequentialNode {
        public INode Next { get; set; }

        public SayNode(IExpression textExpression, Speaker speaker) : base(textExpression, speaker) { }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return Next;
        }

        public INode GetSingleNextNode(StateContainer state) {
            return Next;
        }
    }
}
