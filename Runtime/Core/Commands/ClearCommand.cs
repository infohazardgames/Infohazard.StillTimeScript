using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ClearCommand : Command, ISequentialCommand {
        public ClearCommand(int lineNumber, string line) : base(lineNumber, line) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            builder.Append(new ClearNode());
        }
    }
}
