using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class BgNode : SequentialNode {
        public IExpression Color { get; }

        public IExpression Time { get; }

        public Variable Variable { get; }

        public BgNode(IExpression color, IExpression time, Variable variable) {
            Color = color;
            Time = time;
            Variable = variable;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            VariablesComponent component = state.GetOrCreate<VariablesComponent>();
            component.SetVariableValue(Variable, Color.Evaluate(state));
        }
    }
}
