using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class StateExplorer : MonoBehaviour {
        public StateAdvancer _stateAdvancer;
        public string _exploredScopeName;

        public bool ExploreBranchForNewContent(
            GameGraph graph,
            List<StateContainer> stack,
            StateContainer state,
            int maxDepth) {

            if (!graph.TryGetResource(_exploredScopeName, out Scope scope)) return false;

            StateContainer previousState = stack[^1];

            INode currentNode = state.GetOrCreate<CurrentNodeComponent>().CurrentNode;
            if (!previousState.GetOrCreate<VisitedNodesComponent>().IsVisited(scope, currentNode)) {
                return true;
            }

            if (stack.Count >= maxDepth) {
                Debug.LogError("Search reached max depth. This should not happen.");
                return true;
            }

            try {
                stack.Add(state);

                foreach (INode possibleNext in currentNode.GetPossibleNextNodes(state)) {
                    if (possibleNext is null or ResetScopeNode) continue;

                    StateContainer previousStateAtNode =
                        stack.FindLast(s => s.GetOrCreate<CurrentNodeComponent>().CurrentNode == possibleNext);

                    if (previousStateAtNode != null &&
                        previousStateAtNode.GetOrCreate<VariablesComponent>().Variables
                                           .SequenceEqual(state.GetOrCreate<VariablesComponent>().Variables)) {
                        continue;
                    }

                    StateContainer nextState = _stateAdvancer.AdvanceState(graph, state, possibleNext);

                    if (ExploreBranchForNewContent(graph, stack, nextState, maxDepth)) return true;
                }
            } finally {
                if (stack[^1] != state) {
                    throw new Exception("Error in stack operation");
                }

                stack.RemoveAt(stack.Count - 1);
            }

            return false;
        }
    }
}
