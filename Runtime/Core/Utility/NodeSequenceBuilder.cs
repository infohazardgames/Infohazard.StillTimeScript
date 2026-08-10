using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Nodes;

namespace Infohazard.StillTimeScript.Core.Utility {
    public class NodeSequenceBuilder {
        private readonly HashSet<INode> _nodeSet = new();
        private readonly List<INode> _nodeList = new();

        public ReadOnlySet<INode> NodeSet { get; }
        public IReadOnlyList<INode> NodeList => _nodeList;

        public INode FirstNode => _nodeList.Count > 0 ? _nodeList[0] : null;
        public INode LastNode => _nodeList.Count > 0 ? _nodeList[^1] : null;
        public ISequentialNode CurrentNode { get; private set; }

        public NodeSequenceBuilder() {
            NodeSet = new ReadOnlySet<INode>(_nodeSet);
        }

        public NodeSequenceBuilder(ISequentialNode current) : this() {
            Append(current);
        }

        public void Append(INode node) {
            if (CurrentNode != null) CurrentNode.Next = node;
            CurrentNode = node as ISequentialNode;

            if (_nodeSet.Add(node)) {
                _nodeList.Add(node);
            }
        }

        public void EndWithExternalNode(INode externalNode) {
            if (CurrentNode != null) CurrentNode.Next = externalNode;
            CurrentNode = null;
        }

        public void End() {
            CurrentNode = null;
        }
    }
}
