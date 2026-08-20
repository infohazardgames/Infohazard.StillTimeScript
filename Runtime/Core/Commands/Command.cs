using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class Command : ICommand {
        public int LineNumber { get; }
        public string Line { get; }

        public Command(LineTokens tokens) {
            LineNumber = tokens.LineNumber;
            Line = tokens.OriginalLine;
        }

        public Command(int lineNumber, string line) {
            LineNumber = lineNumber;
            Line = line;
        }

        public virtual void GatherSubCommands(ref CommandGatheringState state) { }

        public virtual IEnumerable<CommandToken> EnumerateTokens() => Enumerable.Empty<CommandToken>();
    }
}
