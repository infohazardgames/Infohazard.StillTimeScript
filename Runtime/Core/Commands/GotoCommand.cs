using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class GotoCommand : Command, ISequentialCommand {
        public string TargetLabel { get; }

        public GotoCommand(int lineNumber, string line, string targetLabel) :
            base(lineNumber, line) {
            TargetLabel = targetLabel;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            INode target = graphData.GetNode(this, TargetLabel);
            GotoNode node = new(target);
            builder.Append(node);
        }
    }
}
