using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("goto", 1)]
    public class GotoCommand : Command, ISequentialCommand {
        public Token TargetStr { get; }

        public GotoCommand(LineTokens tokens) : base(tokens) {
            TargetStr = tokens.GetRequiredArg(0);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            INode target = graphData.GetNode(this, TargetStr.Text);
            GotoNode node = new(target);
            builder.Append(node);
        }
    }
}
