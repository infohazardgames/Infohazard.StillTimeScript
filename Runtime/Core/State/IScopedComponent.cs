using Infohazard.StillTimeScript.Core.Resource;

namespace Infohazard.StillTimeScript.Core.State {
    public interface IScopedComponent : IStateComponent {
        public void ResetScope(Scope scope);
    }
}
