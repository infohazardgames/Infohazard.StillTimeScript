using System;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class BinaryMathExpression : IExpression {
        public StsValueType Type => StsValueType.Number;
        
        public BinaryMathOperator Operator { get; }
        
        public IExpression Left { get; }
        public IExpression Right { get; }
        
        public BinaryMathExpression(IExpression left, IExpression right, BinaryMathOperator op) {
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
            
            decimal result = Operator switch {
                BinaryMathOperator.Add => lhs + rhs,
                BinaryMathOperator.Subtract => lhs - rhs,
                BinaryMathOperator.Multiply => lhs * rhs,
                BinaryMathOperator.Divide => lhs / rhs,
                BinaryMathOperator.Modulo => lhs % rhs,
                BinaryMathOperator.Power => (decimal)Math.Pow((double)lhs, (double)rhs),
                _ => throw new Exception("Unknown number comparison operator"),
            };

            return new StsValue(result);
        }
    }

    public enum BinaryMathOperator {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Power,
    }
}