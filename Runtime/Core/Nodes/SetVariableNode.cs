using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class SetVariableNode : SequentialNode {
        public Variable Variable { get; }

        public IExpression Value { get; }

        public SetVariableNode(Variable variable, IExpression value) {
            Variable = variable;
            Value = value;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            state.GetOrCreate<VariablesComponent>().SetVariableValue(Variable, Value.Evaluate(state));
        }
    }
}
