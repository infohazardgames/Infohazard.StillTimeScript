using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class Choice : IBranchOption {
        public string Text { get; }

        public INode Next { get; }

        public IExpression Condition { get; }

        public Choice(string text, INode next, IExpression condition = null) {
            Text = text;
            Next = next;
            Condition = condition;
        }

        public virtual string GetText(StateContainer state) {
            return Text;
        }

        public virtual bool IsAvailable(StateContainer state) {
            if (Condition != null && !Condition.Evaluate(state).ToBool()) return false;
            return true;
        }

        public virtual INode GetNextNode(StateContainer state) {
            return Next;
        }
    }
}