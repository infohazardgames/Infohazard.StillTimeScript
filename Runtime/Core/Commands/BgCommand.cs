using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("bg", 1, 2)]
    public class BgCommand : Command, ISequentialCommand, IResourceCommand {
        public const string BuiltInVariableName = "__BuiltIn_BgCommand_Color";

        public string ColorStr { get; }
        public string TimeStr { get; }

        public BgCommand(LineTokens tokens) : base(tokens) {
            ColorStr = tokens.GetArg(0);
            TimeStr = tokens.GetArg(1);
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
            IExpression colorExpr = ExpressionParser.ParseExpression(this, graphData, ColorStr, StsValueType.Color);
            IExpression timeExpr = string.IsNullOrEmpty(TimeStr)
                ? null
                : ExpressionParser.ParseExpression(this, graphData, TimeStr, StsValueType.Number);

            Variable variable = graphData.GetResource<Variable>(this, BuiltInVariableName);
            BgNode node = new(colorExpr, timeExpr, variable);
            builder.Append(node);
        }
    }
}
