using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Newtonsoft.Json.Linq;

namespace Infohazard.StillTimeScript.Core.State {
    public class CurrentNodeComponent : IStateComponent {
        public INode CurrentNode { get; set; }

        public List<INode> NodeStack { get; } = new();

        public IStateComponent Clone() {
            CurrentNodeComponent clone = new() { CurrentNode = CurrentNode };
            clone.NodeStack.AddRange(NodeStack);
            return clone;
        }

        public JToken Serialize() {
            SerializedData data = new() {
                CurrentNode = CurrentNode.FullIdentifier,
                Stack = NodeStack.Select(n => n.FullIdentifier).ToList(),
            };

            return JObject.FromObject(data);
        }

        public bool Deserialize(GameGraph graph, JToken token) {
            SerializedData data = token.ToObject<SerializedData>();

            if (!graph.TryGetNode(data.CurrentNode, out INode node)) return false;
            CurrentNode = node;

            NodeStack.Clear();
            if (data.Stack != null) {
                foreach (string s in data.Stack) {
                    if (!graph.TryGetNode(s, out INode stackNode)) continue;
                    NodeStack.Add(stackNode);
                }
            }

            return true;
        }

        private class SerializedData {
            public string CurrentNode { get; set; }
            public List<string> Stack { get; set; }
        }
    }
}
