using System;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class NumberCompareExpression : IExpression {
        public StsValueType Type => StsValueType.Bool;
        
        public NumberComparisonOperator Operator { get; }
        
        public IExpression Left { get; }
        public IExpression Right { get; }
        
        public NumberCompareExpression(IExpression left, IExpression right, NumberComparisonOperator op) {
            if (left.Type != StsValueType.Number || right.Type != StsValueType.Number) {
                throw new ArgumentException("Both expressions must be of type number");
            }

            Left = left;
            Right = right;
            Operator = op;
        }

        public StsValue Evaluate(StateContainer state) {
            decimal lhs = Left.Evaluate(state).NumberValue;
            decimal rhs = Right.Evaluate(state).NumberValue;
            
            bool result = Operator switch {
                NumberComparisonOperator.Greater => lhs > rhs,
                NumberComparisonOperator.Less => lhs < rhs,
                NumberComparisonOperator.GreaterOrEqual => lhs >= rhs,
                NumberComparisonOperator.LessOrEqual => lhs <= rhs,
                _ => throw new Exception("Unknown number comparison operator"),
            };

            return new StsValue(result);
        }
    }

    public enum NumberComparisonOperator {
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual,
    }
}