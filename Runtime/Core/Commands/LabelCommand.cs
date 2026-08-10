using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class LabelCommand : Command, IResourceCommand, ISubtreeCommand {
        public string Identifier { get; }

        public List<ISequentialCommand> Commands { get; } = new();

        public LabelCommand(int lineNumber, string line, string identifier) : base(lineNumber, line) {
            Identifier = identifier;
        }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, Commands);
        }

        public void CreateResources(GraphData graphData) {
            EmptyNode rootNode = new() { FullIdentifier = Identifier };
            graphData.Nodes.Add(Identifier, rootNode);
        }

        public void BuildSubtree(GraphData graphData) {
            if (!graphData.Nodes.TryGetValue(Identifier, out INode node) || node is not EmptyNode emptyNode) {
                throw new Exception($"Could not find empty node for label {Identifier} in provided dictionary.");
            }

            NodeSequenceBuilder builder = new();
            foreach (ISequentialCommand command in Commands) {
                command.ApplyToSequence(builder, graphData);
            }

            CommandUtility.AssignIds(Identifier, builder, graphData);
            emptyNode.Next = builder.FirstNode;
        }
    }
}
