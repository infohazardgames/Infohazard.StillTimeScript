using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("clear")]
    public class ClearCommand : Command, ISequentialCommand {
        public ClearCommand(LineTokens tokens) : base(tokens) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            builder.Append(new ClearNode());
        }
    }
}
