using System.Linq;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class ResetScopeNode : SequentialNode {
        public Scope Scope { get; }

        public ResetScopeNode(Scope scope) {
            Scope = scope;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            foreach (IScopedComponent scopedComponent in state.Components.Values.OfType<IScopedComponent>()) {
                scopedComponent.ResetScope(Scope);
            }
        }
    }
}
