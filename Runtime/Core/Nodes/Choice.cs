using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class Choice : IBranchOption {
        public IExpression TextExpression { get; }

        public IExpression Next { get; }

        public IExpression Condition { get; }

        public Choice(IExpression textExpression, IExpression next, IExpression condition = null) {
            TextExpression = textExpression;
            Next = next;
            Condition = condition;
        }

        public virtual string GetText(StateContainer state) {
            return TextExpression.Evaluate(state).StringValue;
        }

        public virtual bool IsAvailable(StateContainer state) {
            if (Condition != null && !Condition.Evaluate(state).ToBool()) return false;
            return true;
        }

        public virtual INode GetNextNode(StateContainer state) {
            return Next.Evaluate(state).NodeValue;
        }
    }
}
