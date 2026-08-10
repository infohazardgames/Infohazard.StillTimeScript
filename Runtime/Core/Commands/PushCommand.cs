using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class PushCommand : Command, ISequentialCommand {
        public string TargetLabel { get; }

        public PushCommand(int lineNumber, string line, string targetLabel) : base(lineNumber, line) {
            TargetLabel = targetLabel;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            INode target = graphData.GetNode(this, TargetLabel);
            PushNode node = new(target);
            builder.Append(node);
        }
    }
}
