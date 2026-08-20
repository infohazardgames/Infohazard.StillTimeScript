using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("speaker", 2, 2, true)]
    public class SpeakerCommand : Command, IResourceCommand {
        public Token Name { get; }
        public Token ColorStr { get; }
        public Token TextExprStr { get; }

        public SpeakerCommand(LineTokens tokens) : base(tokens) {
            Name = tokens.GetRequiredArg(0);
            ColorStr = tokens.GetRequiredArg(1);
            TextExprStr = tokens.GetRequiredText();
        }

        public void CreateResources(GraphData graphData) {
            Speaker speaker = new(Name.Text);
            graphData.Resources.Add(Name.Text, speaker);
        }

        public void ValidateResources(GraphData graphData) {
            IExpression colorExpr =
                ExpressionParser.ParseExpression(this, graphData, Line, ColorStr.Range, StsValueType.Color);
            IExpression textExpr =
                ExpressionParser.ParseStringExpression(this, graphData, Line, TextExprStr.Range);

            Speaker speaker = graphData.GetResource<Speaker>(this, Name.Text);
            speaker.Color = colorExpr;
            speaker.Text = textExpr;
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(Name, CommandTokenType.Definition);
            yield return new CommandToken(ColorStr, CommandTokenType.Expression, StsValueType.Color);
            yield return new CommandToken(TextExprStr, CommandTokenType.StringExpression);
        }
    }
}
