using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class StateExplorer : MonoBehaviour {
        public StateAdvancer _stateAdvancer;
        public string _exploredScopeName;
        public string[] _ignoredVariables;

        private HashSet<string> _ignoredVariablesSet;

        private void Awake() {
            _ignoredVariablesSet = new HashSet<string>(_ignoredVariables);
        }

        public bool ExploreBranchForNewContent(
            GameGraph graph,
            List<StateContainer> stack,
            StateContainer state,
            int maxDepth) {

            if (!graph.TryGetResource(_exploredScopeName, out Scope scope)) return false;

            StateContainer previousState = stack[^1];

            CurrentNodeComponent currentNodeComponent = state.GetOrCreate<CurrentNodeComponent>();
            INode currentNode = currentNodeComponent.CurrentNode;
            if (!previousState.GetOrCreate<VisitedNodesComponent>().IsVisited(scope, currentNode)) {
                return true;
            }

            if (stack.Count >= maxDepth) {
                Debug.LogError("Search reached max depth. This should not happen.");
                return true;
            }

            try {
                stack.Add(state);
                VariablesComponent stateVariables = state.GetOrCreate<VariablesComponent>();

                foreach (INode possibleNext in currentNode.GetPossibleNextNodes(state)) {
                    if (possibleNext is ResetScopeNode) continue;

                    if (!_stateAdvancer.TryAdvanceState(graph, state, possibleNext, out StateContainer nextState)) {
                        continue;
                    }

                    StateContainer previousStateAtNode =
                        stack.FindLast(s => s.GetOrCreate<CurrentNodeComponent>().CurrentNode == possibleNext);

                    VariablesComponent previousVariables = previousStateAtNode?.GetOrCreate<VariablesComponent>();

                    if (previousVariables != null && VariablesAreEqual(stateVariables, previousVariables)) {
                        continue;
                    }

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

        private bool VariablesAreEqual(VariablesComponent var1, VariablesComponent var2) {
            Dictionary<Variable, StsValue>.KeyCollection keys1 = var1.Variables.Keys;
            Dictionary<Variable, StsValue>.KeyCollection keys2 = var2.Variables.Keys;

            HashSet<Variable> variables = new(keys1.Concat(keys2));

            foreach (Variable variable in variables) {
                if (_ignoredVariablesSet.Contains(variable.Identifier)) continue;

                if (!var1.Variables.TryGetValue(variable, out StsValue value1) ||
                    !var2.Variables.TryGetValue(variable, out StsValue value2) ||
                    !value1.Equals(value2)) {
                    return false;
                }
            }

            return true;
        }
    }
}
