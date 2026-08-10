using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Game.StateProcessors {
    public class InterruptStateProcessor : StateProcessor {
        public override void ProcessBeforeAdvance(GameGraph graph, StateContainer state, ref INode nextNode) {
            foreach (Interrupt interrupt in graph.ResourcesByIdentifier.Values.OfType<Interrupt>()) {
                if (!interrupt.Condition.Evaluate(state).ToBool()) continue;

                nextNode = interrupt.TargetNode;
                return;
            }
        }
    }
}