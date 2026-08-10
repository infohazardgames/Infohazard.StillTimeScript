using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class VariableExpression : IExpression {
        public StsValueType Type => Variable.Type;
        
        public Variable Variable { get; }
        
        public VariableExpression(Variable variable) {
            Variable = variable;
        }
        
        public StsValue Evaluate(StateContainer state) {
            return state.GetOrCreate<VariablesComponent>().GetVariableValue(Variable);
        }
    }
}