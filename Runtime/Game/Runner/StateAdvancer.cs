using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.StateProcessors;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class StateAdvancer : MonoBehaviour {
        public List<StateProcessor> _stateProcessors;

        public StateContainer AdvanceState(GameGraph graph, StateContainer currentState, INode nextNode) {
            StateContainer newState = currentState.Clone();

            foreach (StateProcessor stateProcessor in _stateProcessors) {
                stateProcessor.ProcessBeforeAdvance(graph, newState, ref nextNode);
            }

            CurrentNodeComponent currentComponent = newState.GetOrCreate<CurrentNodeComponent>();
            VisitedNodesComponent visitedComponent = newState.GetOrCreate<VisitedNodesComponent>();

            currentComponent.CurrentNode.ApplyBeforeAdvanceFromSelf(graph, newState, ref nextNode);
            currentComponent.CurrentNode = nextNode;
            visitedComponent.VisitNode(nextNode, true);
            nextNode?.ApplyAfterAdvanceToSelf(graph, newState);

            foreach (StateProcessor stateProcessor in _stateProcessors) {
                stateProcessor.ProcessAfterAdvance(graph, newState);
            }

            return newState;
        }
    }
}
