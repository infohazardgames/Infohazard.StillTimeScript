using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class BgNode : SequentialNode {
        public StsColor Color { get; }

        public float Time { get; }

        public Variable Variable { get; }

        public BgNode(StsColor color, float time, Variable variable) {
            Color = color;
            Time = time;
            Variable = variable;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            VariablesComponent component = state.GetOrCreate<VariablesComponent>();
            component.SetVariableValue(Variable, new StsValue(Color));
        }
    }
}
