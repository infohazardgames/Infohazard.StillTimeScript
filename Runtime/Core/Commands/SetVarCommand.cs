using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("set", 2, 2)]
    public class SetVarCommand : Command, ISequentialCommand {
        public Token VarName { get; }
        public Token ValueStr { get; }

        public SetVarCommand(LineTokens tokens) : base(tokens) {
            VarName = tokens.GetRequiredArg(0);
            ValueStr = tokens.GetRequiredArg(1);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, VarName.Text);
            IExpression expression = ExpressionParser.ParseExpression(this, graphData, Line, ValueStr.Range, variable.Type);
            SetVariableNode setVariableNode = new(variable, expression);
            builder.Append(setVariableNode);
        }
    }
}
