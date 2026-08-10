using System;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class EqualityExpression : IExpression {
        
        public StsValueType Type { get; }
        
        public EqualityOperator Operator { get; }
        
        public IExpression Left { get; }
        public IExpression Right { get; }
        
        public EqualityExpression(IExpression left, IExpression right, EqualityOperator op) {
            if (left.Type != right.Type) {
                throw new ArgumentException("Both expressions must be of the same type");
            }

            Type = left.Type;
            Left = left;
            Right = right;
            Operator = op;
        }
        
        public StsValue Evaluate(StateContainer state) {
            StsValue lhs = Left.Evaluate(state);
            StsValue rhs = Right.Evaluate(state);
            
            bool result = Operator switch {
                EqualityOperator.Equal => lhs.Equals(rhs),
                EqualityOperator.NotEqual => !lhs.Equals(rhs),
                _ => throw new Exception("Unknown equality operator"),
            };

            return new StsValue(result);
        }
    }

    public enum EqualityOperator {
        Equal,
        NotEqual,
    }
}