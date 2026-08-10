using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ElseCommand : Command {
        public List<ISequentialCommand> Commands { get; } = new();

        public ElseCommand(int lineNumber, string line) : base(lineNumber, line) { }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, Commands);
        }
    }
}
