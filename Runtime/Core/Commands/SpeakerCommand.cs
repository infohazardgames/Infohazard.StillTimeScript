using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class SpeakerCommand : Command, IResourceCommand {
        public string Name { get; }
        public StsColor Color { get; }
        public string Text { get; }

        public SpeakerCommand(int lineNumber, string line, string name, StsColor color, string text) :
            base(lineNumber, line) {
            Name = name;
            Color = color;
            Text = text;
        }

        public void CreateResources(GraphData graphData) {
            Speaker speaker = new(Name, Color, Text);
            graphData.Resources.Add(Name, speaker);
        }
    }
}
