using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("label", 1)]
    public class LabelCommand : Command, IResourceCommand, ISubtreeCommand {
        public Token Identifier { get; }

        public List<ISequentialCommand> Commands { get; } = new();

        public LabelCommand(LineTokens tokens) : base(tokens) {
            Identifier = tokens.GetRequiredArg(0);
        }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, Commands);
        }

        public void CreateResources(GraphData graphData) {
            EmptyNode rootNode = new() { FullIdentifier = Identifier.Text };
            graphData.Nodes.Add(Identifier.Text, rootNode);
        }

        public void BuildSubtree(GraphData graphData) {
            if (!graphData.Nodes.TryGetValue(Identifier.Text, out INode node) || node is not EmptyNode emptyNode) {
                throw new Exception($"Could not find empty node for label {Identifier} in provided dictionary.");
            }

            NodeSequenceBuilder builder = new();
            foreach (ISequentialCommand command in Commands) {
                command.ApplyToSequence(builder, graphData);
            }

            CommandUtility.AssignIds(Identifier.Text, builder, graphData);
            emptyNode.Next = builder.FirstNode;
        }
    }
}
