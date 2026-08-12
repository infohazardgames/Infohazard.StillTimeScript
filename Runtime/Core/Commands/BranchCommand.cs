using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("branch", 0, 1, true)]
    public class BranchCommand : TextCommand, ISequentialCommand {
        public List<IBranchSubCommand> SubCommands { get; } = new();

        public BranchCommand(LineTokens tokens) : base(tokens, tokens.GetArg(0), tokens.Text) { }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, SubCommands);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Speaker speaker = GetSpeaker(graphData);
            BranchNode branchNode = new(GetTextExpression(graphData), speaker);

            foreach (IBranchSubCommand subCommand in SubCommands) {
                subCommand.CreateBranchOptions(graphData, branchNode.Options);
            }

            builder.Append(branchNode);
        }
    }
}
