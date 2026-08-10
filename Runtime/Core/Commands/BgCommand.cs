using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class BgCommand : Command, ISequentialCommand, IResourceCommand {
        public const string BuiltInVariableName = "__BuiltIn_BgCommand_Color";

        public StsColor Color { get; }
        public float Time { get; }

        public BgCommand(int lineNumber, string line, StsColor color, float time) : base(lineNumber, line) {
            Color = color;
            Time = time;
        }

        public void CreateResources(GraphData graphData) {

            if (graphData.Resources.TryGetValue(BuiltInVariableName, out Resource.Resource resource)) {
                if (resource is not Variable { Type: StsValueType.Color }) {
                    throw new ParsingException(
                        LineNumber,
                        Line,
                        $"Built-in variable name {BuiltInVariableName} already exists and is not of correct type.");
                } else {
                    return;
                }
            }

            graphData.Resources[BuiltInVariableName] =
                new Variable(BuiltInVariableName, StsValueType.Color, null, StsValue.Default(StsValueType.Color));
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Variable variable = graphData.GetResource<Variable>(this, BuiltInVariableName);
            BgNode node = new(Color, Time, variable);
            builder.Append(node);
        }
    }
}
