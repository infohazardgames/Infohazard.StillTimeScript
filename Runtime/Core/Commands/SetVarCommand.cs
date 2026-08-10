using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class SetVarCommand : Command, ISequentialCommand {
        public string VarName { get; }
        public string Value { get; }

        public SetVarCommand(int lineNumber, string line, string varName, string value) : base(lineNumber, line) {
            VarName = varName;
            Value = value;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, VarName);
            if (!StsValue.TryParse(Value, variable.Type, out StsValue varValue)) {
                throw new ParsingException(LineNumber, Line, $"Invalid value {Value} for var type {variable.Type}");
            }

            SetVariableNode setVariableNode = new(variable, varValue);
            builder.Append(setVariableNode);
        }
    }
}
