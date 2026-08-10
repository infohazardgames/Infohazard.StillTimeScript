using System;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class SetVariableNode : SequentialNode {
        public Variable Variable { get; }

        public StsValue Value { get; }

        public SetVariableNode(Variable variable, StsValue value) {
            if (value.ValueType != variable.Type) {
                throw new Exception($"Invalid value for variable {variable.Identifier}: {value}");
            }

            Variable = variable;
            Value = value;
        }

        public override void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) {
            state.GetOrCreate<VariablesComponent>().SetVariableValue(Variable, Value);
        }
    }
}
