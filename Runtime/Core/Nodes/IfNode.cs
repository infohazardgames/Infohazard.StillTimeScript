using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class IfNode : Node, ISingleNextNode {
        public IExpression Condition { get; }

        public INode TrueBranch { get; set; }

        public INode FalseBranch { get; set; }

        public IfNode(IExpression condition) {
            Condition = condition;
        }

        public INode GetSingleNextNode(StateContainer state) {
            if (Condition.Evaluate(state).ToBool()) {
                return TrueBranch;
            } else {
                return FalseBranch;
            }
        }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return GetSingleNextNode(state);
        }
    }
}
