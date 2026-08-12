using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Resource;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public abstract class TextNode : Node {
        public IExpression TextExpression { get; }
        public Speaker Speaker { get; }

        public TextNode(IExpression textExpression, Speaker speaker) {
            TextExpression = textExpression;
            Speaker = speaker;
        }

        public override string GetSelfIdentifier() {
            if (Speaker == null) {
                return base.GetSelfIdentifier();
            } else {
                return $"{base.GetSelfIdentifier()}({Speaker.Identifier})";
            }
        }
    }
}
