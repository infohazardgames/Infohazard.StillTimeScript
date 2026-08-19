using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("interrupt", 3, 3)]
    public class InterruptCommand : Command, IResourceCommand {
        public Token InterruptId { get; }
        public Token TargetStr { get; }
        public Token ConditionStr { get; }

        public InterruptCommand(LineTokens tokens) : base(tokens) {
            InterruptId = tokens.GetRequiredArg(0);
            TargetStr = tokens.GetRequiredArg(1);
            ConditionStr = tokens.GetRequiredArg(2);
        }

        public void CreateResources(GraphData graphData) {
            graphData.Resources.Add(InterruptId.Text, new Interrupt(InterruptId.Text));
        }

        public void ValidateResources(GraphData graphData) {
            Interrupt interrupt = graphData.GetResource<Interrupt>(this, InterruptId.Text);

            IExpression condition = ExpressionParser.ParseExpression(this, graphData, Line, ConditionStr.Range);
            IExpression target =
                ExpressionParser.ParseExpression(this, graphData, Line, TargetStr.Range, StsValueType.Node);
            interrupt.Condition = condition;
            interrupt.Target = target;
        }
    }
}
