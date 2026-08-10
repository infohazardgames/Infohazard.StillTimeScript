using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("interrupt")]
    public class InterruptCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 3, 3, false);
            InterruptCommand command = new(
                tokens.LineNumber,
                tokens.OriginalLine,
                tokens.Arguments[0],
                tokens.Arguments[1], 
                tokens.Arguments[2]);
            commands.Add(command);
        }
    }
}
