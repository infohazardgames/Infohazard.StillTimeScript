using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class IncrVarCommand : Command, ISequentialCommand {
        public string VarName { get; }

        public decimal Value { get; }

        public IncrVarCommand(int lineNumber, string line, string varName, decimal value) : base(lineNumber, line) {
            VarName = varName;
            Value = value;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, VarName);

            if (variable.Type != StsValueType.Number) {
                throw new ParsingException(LineNumber, Line, "Increment is only valid for number variable");
            }

            IncrementVariableNode incrementVariableNode = new(variable, Value);
            builder.Append(incrementVariableNode);
        }
    }
}
