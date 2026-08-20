using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("incr", 2, 2)]
    public class IncrVarCommand : Command, ISequentialCommand {
        public Token VarName { get; }

        public Token ValueStr { get; }

        public IncrVarCommand(LineTokens tokens) : base(tokens) {
            VarName = tokens.GetRequiredArg(0);
            ValueStr = tokens.GetRequiredArg(1);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, VarName.Text);

            if (variable.Type != StsValueType.Number) {
                throw new ParsingException(LineNumber, Line, "Increment is only valid for number variable");
            }

            IExpression expression =
                ExpressionParser.ParseExpression(this, graphData, Line, ValueStr.Range, StsValueType.Number);
            IncrementVariableNode incrementVariableNode = new(variable, expression);
            builder.Append(incrementVariableNode);
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(VarName, CommandTokenType.ResourceReference, StsValueType.Number);
            yield return new CommandToken(ValueStr, CommandTokenType.Expression, StsValueType.Number);
        }
    }
}
