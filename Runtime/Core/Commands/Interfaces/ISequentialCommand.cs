using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands.Interfaces {
    public interface ISequentialCommand : ICommand {
        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData);
    }
}
