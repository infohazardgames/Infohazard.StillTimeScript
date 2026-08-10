using System;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class UnaryLogicExpression : IExpression {
        public StsValueType Type => StsValueType.Bool;
        public UnaryLogicOperator Operator { get; }
        
        private IExpression SubExpression { get; }
        
        public UnaryLogicExpression(UnaryLogicOperator op, IExpression subExpression) {
            Operator = op;
            SubExpression = subExpression;
        }
        
        public StsValue Evaluate(StateContainer state) {
            StsValue subResult = SubExpression.Evaluate(state);
            return Operator switch {
                UnaryLogicOperator.Not => new StsValue(!subResult.ToBool()),
                _ => throw new Exception("Unknown unary logic operator"),
            };
        }
    }
    
    public enum UnaryLogicOperator {
        Not,
    }
}