using Infohazard.StillTimeScript.Core.Expressions;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class DelayNode : SequentialNode {
        public IExpression Time { get; }

        public DelayNode(IExpression time) {
            Time = time;
        }
    }
}
