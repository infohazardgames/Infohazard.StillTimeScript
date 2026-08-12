using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("pop")]
    public class PopCommand : Command, ISequentialCommand {
        public PopCommand(LineTokens tokens) : base(tokens) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            builder.Append(new PopNode());
        }
    }
}
