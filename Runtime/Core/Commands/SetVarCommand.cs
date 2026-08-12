using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("set", 2, 2)]
    public class SetVarCommand : Command, ISequentialCommand {
        public string VarName { get; }
        public string ValueStr { get; }

        public SetVarCommand(LineTokens tokens) : base(tokens) {
            VarName = tokens.GetArg(0);
            ValueStr = tokens.GetArg(1);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, VarName);
            IExpression expression = ExpressionParser.ParseExpression(this, graphData, ValueStr, variable.Type);
            SetVariableNode setVariableNode = new(variable, expression);
            builder.Append(setVariableNode);
        }
    }
}
