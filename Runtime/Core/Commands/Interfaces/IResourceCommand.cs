using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands.Interfaces {
    public interface IResourceCommand : ICommand {
        public void CreateResources(GraphData graphData);

        public void ValidateResources(GraphData graphData) { }
    }
}
