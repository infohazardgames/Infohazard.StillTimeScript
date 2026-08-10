using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.StateProcessors {
    public abstract class StateProcessor : MonoBehaviour {
        public virtual void ProcessBeforeAdvance(GameGraph graph, StateContainer state, ref INode nextNode) { }
        public virtual void ProcessAfterAdvance(GameGraph graph, StateContainer state) { }
    }
}
