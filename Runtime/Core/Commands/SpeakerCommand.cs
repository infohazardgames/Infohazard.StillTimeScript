using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("speaker", 1, 2, true)]
    public class SpeakerCommand : Command, IResourceCommand {
        public string Name { get; }
        public string ColorStr { get; }
        public string TextExprStr { get; }

        public SpeakerCommand(LineTokens tokens) : base(tokens) {
            Name = tokens.GetArg(0);
            ColorStr = tokens.GetArg(1);
            TextExprStr = tokens.Text;
        }

        public void CreateResources(GraphData graphData) {
            Speaker speaker = new(Name);
            graphData.Resources.Add(Name, speaker);
        }

        public void ValidateResources(GraphData graphData) {
            IExpression colorExpr = ExpressionParser.ParseExpression(this, graphData, ColorStr, StsValueType.Color);
            IExpression textExpr = ExpressionParser.ParseStringExpression(this, graphData, TextExprStr);

            Speaker speaker = graphData.GetResource<Speaker>(this, Name);
            speaker.Color = colorExpr;
            speaker.Text = textExpr;
        }
    }
}
