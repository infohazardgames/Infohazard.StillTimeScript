using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.StateProcessors;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class StateAdvancer : MonoBehaviour {
        public List<StateProcessor> _stateProcessors;

        public bool TryAdvanceState(GameGraph graph, StateContainer currentState, INode nextNode,
                                    out StateContainer nextState) {
            StateContainer newState = currentState.Clone();

            CurrentNodeComponent currentComponent = newState.GetOrCreate<CurrentNodeComponent>();

            if (nextNode == null) {
                if (currentComponent.NodeStack.Count > 0) {
                    nextNode = currentComponent.NodeStack[^1];
                    currentComponent.NodeStack.RemoveAt(currentComponent.NodeStack.Count - 1);
                } else {
                    nextState = null;
                    return false;
                }
            }

            foreach (StateProcessor stateProcessor in _stateProcessors) {
                stateProcessor.ProcessBeforeAdvance(graph, newState, ref nextNode);
            }

            VisitedNodesComponent visitedComponent = newState.GetOrCreate<VisitedNodesComponent>();

            currentComponent.CurrentNode.ApplyBeforeAdvanceFromSelf(graph, newState, ref nextNode);
            currentComponent.CurrentNode = nextNode;

            if (nextNode != null) {
                visitedComponent.VisitNode(nextNode, true);
                nextNode.ApplyAfterAdvanceToSelf(graph, newState);

                foreach (StateProcessor stateProcessor in _stateProcessors) {
                    stateProcessor.ProcessAfterAdvance(graph, newState);
                }
            }

            nextState = newState;
            return true;
        }
    }
}
