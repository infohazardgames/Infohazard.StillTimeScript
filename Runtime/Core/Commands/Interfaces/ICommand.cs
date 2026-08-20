using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands.Interfaces {
    public interface ICommand {
        public int LineNumber { get; }

        public string Line { get; }

        public void GatherSubCommands(ref CommandGatheringState state);

        public IEnumerable<CommandToken> EnumerateTokens();
    }
}
