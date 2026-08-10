using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ChoiceCommand : TextCommand, IBranchSubCommand {
        public string TargetLabel { get; }
        public string Condition { get; }

        public ChoiceCommand(
            int lineNumber,
            string line,
            string text,
            string targetLabel,
            string condition = null) :
            base(lineNumber, line, null, text) {
            TargetLabel = targetLabel;
            Condition = condition;
        }

        public void CreateBranchOptions(GraphData graphData, List<IBranchOption> options) {
            INode targetNode = graphData.GetNode(this, TargetLabel);

            IExpression expression = string.IsNullOrEmpty(Condition)
                ? null
                : ExpressionParser.ParseExpression(this, graphData, Condition);
            
            Choice choice = new(Text, targetNode, expression);

            options.Add(choice);
        }
    }
}
