using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Newtonsoft.Json.Linq;

namespace Infohazard.StillTimeScript.Core.State {
    public class VisitedNodesComponent : IScopedComponent {
        private readonly Dictionary<Scope, ScopeInfo> _dictionary;

        public VisitedNodesComponent() {
            _dictionary = new Dictionary<Scope, ScopeInfo>();
        }

        private VisitedNodesComponent(Dictionary<Scope, ScopeInfo> dictionary) {
            _dictionary = dictionary;
        }

        public void Initialize(GameGraph gameGraph) {
            foreach (Scope scope in gameGraph.ResourcesByIdentifier.Values.OfType<Scope>()) {
                if (_dictionary.ContainsKey(scope)) continue;
                _dictionary[scope] = new ScopeInfo { VisitedNodes = new HashSet<INode>() };
            }
        }

        public void VisitNode(INode node, bool updateUnexplored) {
            foreach (ScopeInfo info in _dictionary.Values) {
                bool unexplored = info.VisitedNodes.Add(node);
                if (updateUnexplored) {
                    info.WasCurrentStateUnexplored = unexplored;
                }
            }
        }

        public void ResetScope(Scope scope) {
            if (!_dictionary.TryGetValue(scope, out ScopeInfo info)) return;
            info.VisitedNodes.Clear();
        }

        public bool IsVisited(Scope scope, INode node) {
            if (!_dictionary.TryGetValue(scope, out ScopeInfo info)) return false;
            return info.VisitedNodes.Contains(node);
        }

        public bool WasCurrentStateUnexplored(Scope scope) {
            if (!_dictionary.TryGetValue(scope, out ScopeInfo info)) return true;
            return info.WasCurrentStateUnexplored;
        }

        public IStateComponent Clone() {
            return new VisitedNodesComponent(_dictionary.ToDictionary(p => p.Key, p => p.Value.Clone()));
        }

        public JToken Serialize() {
            Dictionary<string, SerializedScopeInfo> data = new();

            foreach ((Scope scope, ScopeInfo info) in _dictionary) {
                data[scope.Identifier] = new SerializedScopeInfo {
                    VisitedNodes = info.VisitedNodes.Select(n => n.FullIdentifier).ToList(),
                    WasCurrentStateUnexplored = info.WasCurrentStateUnexplored,
                };
            }

            return JToken.FromObject(data);
        }

        public bool Deserialize(GameGraph graph, JToken token) {
            Dictionary<string, SerializedScopeInfo> data = token.ToObject<Dictionary<string, SerializedScopeInfo>>();

            foreach ((string key, SerializedScopeInfo dataItem) in data) {
                if (!graph.TryGetResource(key, out Scope scope) ||
                    !_dictionary.TryGetValue(scope, out ScopeInfo scopeInfo)) continue;

                scopeInfo.WasCurrentStateUnexplored = dataItem.WasCurrentStateUnexplored;
                if (dataItem.VisitedNodes == null) continue;
                foreach (string visitedNodeId in dataItem.VisitedNodes) {
                    if (!graph.TryGetNode(visitedNodeId, out INode node)) continue;
                    scopeInfo.VisitedNodes.Add(node);
                }
            }

            return true;
        }

        private class ScopeInfo {
            public HashSet<INode> VisitedNodes;
            public bool WasCurrentStateUnexplored;

            public ScopeInfo Clone() {
                return new ScopeInfo {
                    VisitedNodes = new HashSet<INode>(VisitedNodes),
                    WasCurrentStateUnexplored = WasCurrentStateUnexplored,
                };
            }
        }

        private struct SerializedScopeInfo {
            public List<string> VisitedNodes { get; set; }
            public bool WasCurrentStateUnexplored { get; set; }
        }
    }
}
