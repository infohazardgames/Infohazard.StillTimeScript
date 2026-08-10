using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class SayCommand : TextCommand, ISequentialCommand {
        public SayCommand(int lineNumber, string line, string speaker, string text) :
            base(lineNumber, line, speaker, text) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Speaker speaker = GetSpeaker(graphData);
            SayNode node = new(Text, speaker);
            builder.Append(node);
        }
    }
}
