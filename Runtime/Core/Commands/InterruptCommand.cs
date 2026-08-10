using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class InterruptCommand : Command, IResourceCommand {
        public string InterruptId { get; }
        public string TargetLabel { get; }
        public string Condition { get; }

        public InterruptCommand(
            int lineNumber,
            string line,
            string interruptId,
            string targetLabel,
            string condition) :
            base(lineNumber, line) {
            InterruptId = interruptId;
            TargetLabel = targetLabel;
            Condition = condition;
        }

        public void CreateResources(GraphData graphData) {
            graphData.Resources.Add(InterruptId, new Interrupt(InterruptId));
        }

        public void ValidateResources(GraphData graphData) {
            Interrupt interrupt = graphData.GetResource<Interrupt>(this, InterruptId);

            IExpression condition = ExpressionParser.ParseExpression(this, graphData, Condition);
            INode target = graphData.GetNode(this, TargetLabel);
            interrupt.Condition = condition;
            interrupt.TargetNode = target;
        }
    }
}
