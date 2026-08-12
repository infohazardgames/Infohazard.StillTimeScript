using System.Collections.Generic;
using System.Text;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class StringConcatExpression : IExpression {
        private readonly List<IExpression> _expressions = new();

        public StsValueType Type => StsValueType.String;

        public void AddExpression(IExpression expression) {
            _expressions.Add(expression);
        }

        public StsValue Evaluate(StateContainer state) {
            StringBuilder builder = new();
            foreach (IExpression expression in _expressions) {
                builder.Append(expression.Evaluate(state).ToString());
            }

            return new StsValue(builder.ToString());
        }
    }
}
