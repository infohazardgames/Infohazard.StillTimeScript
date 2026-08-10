using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class DelayCommand : Command, ISequentialCommand {
        public float Time { get; }

        public DelayCommand(int lineNumber, string line, float time) : base(lineNumber, line) {
            Time = time;
        }

        public void ApplyToSequence(ref ISequentialNode nextNode,
                                    Dictionary<string, Resource.Resource> resourceDictionary,
                                    Dictionary<string, INode> nodeDictionary,
                                    List<INode> createdNodes) {
            DelayNode node = new(Time);
            createdNodes.Add(node);
            nextNode.Next = node;
            nextNode = node;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            builder.Append(new DelayNode(Time));
        }
    }
}
