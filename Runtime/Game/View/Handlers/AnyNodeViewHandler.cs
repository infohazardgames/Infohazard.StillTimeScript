using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public abstract class AnyNodeViewHandler : MonoBehaviour {
        public abstract void HandleState(GameGraph graph, StateContainer state);

        public virtual void HandleInitialState(GameGraph graph, StateContainer state) { }
    }
}
