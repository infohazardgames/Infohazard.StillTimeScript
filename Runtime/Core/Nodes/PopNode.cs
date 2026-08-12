using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class PopNode : SequentialNode {

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return GetSingleNextNode(state);
        }

        public override INode GetSingleNextNode(StateContainer state) {
            CurrentNodeComponent component = state.GetOrCreate<CurrentNodeComponent>();
            if (component.NodeStack.Count > 0) {
                return component.NodeStack[^1];
            } else {
                return Next;
            }
        }

        public override void ApplyBeforeAdvanceFromSelf(GameGraph graph, StateContainer state, ref INode nextNode) {
            CurrentNodeComponent component = state.GetOrCreate<CurrentNodeComponent>();

            if (component.NodeStack.Count > 0) {
                component.NodeStack.RemoveAt(component.NodeStack.Count - 1);
            }
        }
    }
}
