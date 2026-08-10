using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class PopCommand : Command, ISequentialCommand {
        public bool IsTryPop { get; }

        public PopCommand(int lineNumber, string line, bool isTryPop) : base(lineNumber, line) {
            IsTryPop = isTryPop;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            PopNode node = new(IsTryPop);
            builder.Append(node);
        }
    }
}
