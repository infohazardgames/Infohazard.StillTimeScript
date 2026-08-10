using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("branch")]
    public class BranchCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 0, 1, true);

            string branchSpeaker = tokens.Arguments?.Length > 0 ? tokens.Arguments[0] : null;
            BranchCommand branchCommand = new(tokens.LineNumber, tokens.OriginalLine, branchSpeaker, tokens.Text);
            commands.Add(branchCommand);
        }
    }
}
