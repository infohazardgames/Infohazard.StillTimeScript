using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class MacroCallPlaceholder : Command {
        private LineTokens Tokens { get; }

        public MacroCallPlaceholder(LineTokens tokens) : base(tokens) {
            Tokens = tokens;
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(Tokens.Command, CommandTokenType.MacroCall);
        }
    }
}
