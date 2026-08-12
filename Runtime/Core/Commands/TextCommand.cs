using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public abstract class TextCommand : Command {
        public string Speaker { get; }
        public string TextExprStr { get; }

        public TextCommand(LineTokens tokens, string speaker, string textExprStr) :
            this(tokens.LineNumber, tokens.OriginalLine, speaker, textExprStr) { }

        public TextCommand(int lineNumber, string line, string speaker, string textExprStr) : base(lineNumber, line) {
            Speaker = speaker;
            TextExprStr = textExprStr;
        }

        public Speaker GetSpeaker(GraphData graphData) {
            if (string.IsNullOrWhiteSpace(Speaker)) {
                return null;
            }

            return graphData.GetResource<Speaker>(this, Speaker);
        }

        protected IExpression GetTextExpression(GraphData graphData) {
            return ExpressionParser.ParseStringExpression(this, graphData, TextExprStr);
        }
    }
}
