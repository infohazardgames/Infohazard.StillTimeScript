using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public interface ISingleNextNode : INode {
        public INode GetSingleNextNode(StateContainer state);
    }
}
