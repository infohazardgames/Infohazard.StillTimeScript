using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class PushNode : SequentialNode{
        public IExpression Target { get; }

        public PushNode(IExpression target) {
            Target = target;
        }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return GetSingleNextNode(state);
        }

        public override INode GetSingleNextNode(StateContainer state) {
            return Target.Evaluate(state).NodeValue;
        }

        public override void ApplyBeforeAdvanceFromSelf(GameGraph graph, StateContainer state, ref INode nextNode) {
            CurrentNodeComponent component = state.GetOrCreate<CurrentNodeComponent>();
            component.NodeStack.Add(Next);
        }
    }
}
