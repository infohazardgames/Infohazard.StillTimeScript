using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct GraphData {
        public readonly Dictionary<string, INode> Nodes;
        public readonly Dictionary<string, Resource.Resource> Resources;
        public readonly HashSet<Type> RequiredStates;

        public GraphData(Dictionary<string, INode> nodes, Dictionary<string, Resource.Resource> resources,
                         HashSet<Type> requiredStates) {
            Nodes = nodes;
            Resources = resources;
            RequiredStates = requiredStates;
        }

        public static GraphData Empty() {
            return new GraphData(new Dictionary<string, INode>(), new Dictionary<string, Resource.Resource>(),
                                 new HashSet<Type>());
        }


        public T GetResource<T>(ICommand command, string name) {
            if (!Resources.TryGetValue(name, out Resource.Resource resource)) {
                throw new ParsingException(command.LineNumber, command.Line, $"Invalid resource name '{name}'");
            }

            if (resource is not T typed) {
                throw new ParsingException(command.LineNumber, command.Line,
                                           $"Resource {name} is wrong type {resource} (expected {typeof(T).Name})");
            }

            return typed;
        }

        public INode GetNode(ICommand command, string name) {
            if (!Nodes.TryGetValue(name, out INode targetNode)) {
                throw new ParsingException(command.LineNumber, command.Line, "Invalid target node");
            }

            return targetNode;
        }
    }
}
