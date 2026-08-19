using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("var", 3, 4)]
    public class VarCommand : Command, IResourceCommand {
        public Token TypeToken { get; }
        public StsValueType Type { get; }
        public Token Name { get; }
        public Token Scope { get; }
        public Token? DefaultValue { get; }

        public VarCommand(LineTokens tokens) : base(tokens) {
            TypeToken = tokens.Arguments[0];
            string typeName = TypeToken.Text;
            Type = typeName switch {
                "number" or "num" => StsValueType.Number,
                "color" => StsValueType.Color,
                "bool" => StsValueType.Bool,
                "string" or "str" => StsValueType.String,
                _ => throw new ParsingException(tokens.LineNumber, tokens.OriginalLine, $"Invalid var type {typeName}"),
            };

            Name = tokens.Arguments[1];
            Scope = tokens.Arguments[2];
            DefaultValue = tokens.Arguments.Length == 4 ? tokens.Arguments[3] : null;
        }

        public void CreateResources(GraphData graphData) {
            StsValue defaultValue;
            if (DefaultValue == null) {
                defaultValue = StsValue.Default(Type);
            } else if (!StsValue.TryParse(DefaultValue.Value.Text, Type, out defaultValue)) {
                throw new ParsingException(
                    LineNumber, Line, $"Failed to parse var default value of type {Type}: '{defaultValue}'");
            }

            Variable variable = new(Name.Text, Type, Scope.Text, defaultValue);
            graphData.Resources.Add(Name.Text, variable);
        }

        public void ValidateResources(GraphData graphData) {
            _ = graphData.GetResource<Scope>(this, Scope.Text);
        }
    }
}
