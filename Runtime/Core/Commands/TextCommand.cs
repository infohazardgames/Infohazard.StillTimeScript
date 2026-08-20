using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public abstract class TextCommand : Command {
        public Token? Speaker { get; }
        public Token TextExprStr { get; }

        public TextCommand(LineTokens tokens, Token? speaker, Token textExprStr) :
            this(tokens.LineNumber, tokens.OriginalLine, speaker, textExprStr) { }

        public TextCommand(int lineNumber, string line, Token? speaker, Token textExprStr) : base(lineNumber, line) {
            Speaker = speaker;
            TextExprStr = textExprStr;
        }

        public Speaker GetSpeaker(GraphData graphData) {
            if (Speaker == null) {
                return null;
            }

            return graphData.GetResource<Speaker>(this, Speaker.Value.Text);
        }

        protected IExpression GetTextExpression(GraphData graphData) {
            return ExpressionParser.ParseStringExpression(this, graphData, Line, TextExprStr.Range);
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            if (Speaker.HasValue) {
                yield return new CommandToken(Speaker.Value, CommandTokenType.Expression, StsValueType.Resource);
            }

            yield return new CommandToken(TextExprStr, CommandTokenType.TextExpression);
        }
    }
}
