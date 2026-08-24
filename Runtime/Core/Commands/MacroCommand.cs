using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Parsers.Macros;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class MacroCommand : Command {
        public Macro Macro { get; }

        public MacroCommand(LineTokens tokens, Macro macro) : base(tokens) {
            Macro = macro;
        }

        public override IEnumerable<CommandToken> EnumerateTokens() => Macro.EnumerateTokens();
    }
}