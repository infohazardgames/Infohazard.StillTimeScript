using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("say", 0, 1, true)]
    public class SayCommand : TextCommand, ISequentialCommand {
        public SayCommand(LineTokens tokens) : base(tokens, tokens.GetArg(0), tokens.Text) { }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Speaker speaker = GetSpeaker(graphData);
            SayNode node = new(GetTextExpression(graphData), speaker);
            builder.Append(node);
        }
    }
}
