using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("end")]
    public class EndCommand : Command, ISequentialCommand {
        public EndCommand(LineTokens tokens) : base(tokens) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            throw new ParsingException(LineNumber, Line, "End command not expected to be applied.");
        }
    }
}
