using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class ConstantExpression : IExpression {
        public StsValueType Type => Value.ValueType;
        
        public StsValue Value { get; }

        public ConstantExpression(StsValue value) {
            Value = value;
        }
        
        public StsValue Evaluate(StateContainer state) => Value;
    }
}