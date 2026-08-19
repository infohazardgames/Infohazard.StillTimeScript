using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ElseIfCommand : Command {
        public Token Condition { get; }

        public List<ISequentialCommand> Commands { get; } = new();

        public ElseIfCommand(int lineNumber, string line, Token condition) : base(lineNumber, line) {
            Condition = condition;
        }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, Commands, false, true);
        }
    }
}
