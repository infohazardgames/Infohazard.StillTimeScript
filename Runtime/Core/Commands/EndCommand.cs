using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class EndCommand : Command, ISequentialCommand {
        public EndCommand(int lineNumber, string line) : base(lineNumber, line) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            throw new ParsingException(LineNumber, Line, "End command not expected to be applied.");
        }
    }
}
