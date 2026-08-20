using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("delay", 1)]
    public class DelayCommand : Command, ISequentialCommand {
        public Token TimeStr { get; }

        public DelayCommand(LineTokens tokens) : base(tokens) {
            TimeStr = tokens.GetRequiredArg(0);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            IExpression timeExpr =
                ExpressionParser.ParseExpression(this, graphData, Line, TimeStr.Range, StsValueType.Number);
            builder.Append(new DelayNode(timeExpr));
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(TimeStr, CommandTokenType.Expression, StsValueType.Number);
        }
    }
}
