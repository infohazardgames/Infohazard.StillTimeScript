using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("push", 1)]
    public class PushCommand : Command, ISequentialCommand {
        public Token TargetStr { get; }

        public PushCommand(LineTokens tokens) : base(tokens) {
            TargetStr = tokens.GetRequiredArg(0);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            IExpression target =
                ExpressionParser.ParseExpression(this, graphData, Line, TargetStr.Range, StsValueType.Node);
            PushNode node = new(target);
            builder.Append(node);
        }
    }
}
