using System;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class IncrementVariableNode : SequentialNode {
        public Variable Variable { get; }

        public IExpression Increment { get; }

        public IncrementVariableNode(Variable variable, IExpression increment) {
            if (variable.Type != StsValueType.Number) {
                throw new Exception("Increment is only valid for number variable.");
            }

            Variable = variable;
            Increment = increment;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            VariablesComponent variables = state.GetOrCreate<VariablesComponent>();
            StsValue previousValue = variables.GetVariableValue(Variable);
            StsValue newValue = new(previousValue.NumberValue + Increment.Evaluate(state).NumberValue);
            variables.SetVariableValue(Variable, newValue);
        }
    }
}
