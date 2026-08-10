using Infohazard.StillTimeScript.Core.Resource;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public abstract class TextNode : Node {
        public string Text { get; }
        public Speaker Speaker { get; }

        public TextNode(string text, Speaker speaker) {
            Text = text;
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