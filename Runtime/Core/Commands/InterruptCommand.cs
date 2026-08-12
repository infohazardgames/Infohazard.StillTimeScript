using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("interrupt", 3, 3)]
    public class InterruptCommand : Command, IResourceCommand {
        public string InterruptId { get; }
        public string TargetStr { get; }
        public string ConditionStr { get; }

        public InterruptCommand(LineTokens tokens) : base(tokens) {
            InterruptId = tokens.GetArg(0);
            TargetStr = tokens.GetArg(1);
            ConditionStr = tokens.GetArg(2);
        }

        public void CreateResources(GraphData graphData) {
            graphData.Resources.Add(InterruptId, new Interrupt(InterruptId));
        }

        public void ValidateResources(GraphData graphData) {
            Interrupt interrupt = graphData.GetResource<Interrupt>(this, InterruptId);

            IExpression condition = ExpressionParser.ParseExpression(this, graphData, ConditionStr);
            IExpression target = ExpressionParser.ParseExpression(this, graphData, TargetStr, StsValueType.Node);
            interrupt.Condition = condition;
            interrupt.Target = target;
        }
    }
}
