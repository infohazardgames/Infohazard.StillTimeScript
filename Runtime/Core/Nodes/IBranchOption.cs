using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public interface IBranchOption {
        public string GetText(StateContainer state);
        public bool IsAvailable(StateContainer state);
        public INode GetNextNode(StateContainer state);
    }
}
