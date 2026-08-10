using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class GameGraph {
        public INode RootNode { get; }

        public IReadOnlyDictionary<string, INode> NodesByIdentifier { get; }

        public IReadOnlyDictionary<string, Resource.Resource> ResourcesByIdentifier { get; }

        public IReadOnlyList<Type> StateComponentTypes { get; }

        public GameGraph(
            INode rootNode,
            IReadOnlyDictionary<string, INode> nodesByIdentifier,
            IReadOnlyDictionary<string, Resource.Resource> resourcesByIdentifier,
            List<Type> stateComponentTypes) {
            RootNode = rootNode;
            NodesByIdentifier = nodesByIdentifier;
            ResourcesByIdentifier = resourcesByIdentifier;
            StateComponentTypes = stateComponentTypes;
        }

        public void Validate() {
            HashSet<INode> seenNodes = new();
            Queue<INode> toExplore = new();
            toExplore.Enqueue(RootNode);

            while (toExplore.TryDequeue(out INode node)) {
                if (!seenNodes.Add(node)) continue;

                if (string.IsNullOrEmpty(node.FullIdentifier)) {
                    StsLibrary.LogError(
                        $"Node {node} has empty identifier. Creation stack trace:\n{node.CreationStackTrace}");
                }
            }
        }

        public StateContainer BuildEmptyState() {
            StateContainer container = new();

            foreach (Type type in StateComponentTypes) {
                IStateComponent component = (IStateComponent)Activator.CreateInstance(type);
                component.Initialize(this);
                container.Set(type, component);
            }

            return container;
        }

        public bool TryGetResource<T>(string name, out T result) where T : Resource.Resource {
            if (ResourcesByIdentifier.TryGetValue(name, out Resource.Resource resource) && resource is T temp) {
                result = temp;
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public bool TryGetNode(string name, out INode node) {
            if (name == null) {
                node = null;
                return false;
            }

            return NodesByIdentifier.TryGetValue(name, out node);
        }
    }
}
