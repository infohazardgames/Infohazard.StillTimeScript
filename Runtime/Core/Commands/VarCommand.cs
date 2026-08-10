using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class VarCommand : Command, IResourceCommand {
        public StsValueType Type { get; }
        public string Name { get; }
        public string Scope { get; }
        public string DefaultValue { get; }

        public VarCommand(int lineNumber, string line, string type, string name, string scope, string defaultValue) :
            base(lineNumber, line) {
            Type = type switch {
                "number" or "num" => StsValueType.Number,
                "color" => StsValueType.Color,
                "bool" => StsValueType.Bool,
                "string" or "str" => StsValueType.String,
                _ => throw new ParsingException(lineNumber, line, $"Invalid var type {type}"),
            };
            Name = name;
            Scope = scope;
            DefaultValue = defaultValue;
        }

        public void CreateResources(GraphData graphData) {

            StsValue defaultValue;
            if (string.IsNullOrEmpty(DefaultValue)) {
                defaultValue = StsValue.Default(Type);
            } else if (!StsValue.TryParse(DefaultValue, Type, out defaultValue)) {
                throw new ParsingException(
                    LineNumber, Line, $"Failed to parse var default value of type {Type}: '{defaultValue}'");
            }

            Variable variable = new(Name, Type, Scope, defaultValue);
            graphData.Resources.Add(Name, variable);
        }

        public void ValidateResources(GraphData graphData) {
            _ = graphData.GetResource<Scope>(this, Scope);
        }
    }
}
