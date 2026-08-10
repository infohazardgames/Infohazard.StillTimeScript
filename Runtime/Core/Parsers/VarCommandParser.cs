using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("var")]
    public class VarCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 3, 4, false);
            string defaultValue = tokens.Arguments.Length == 4 ? tokens.Arguments[3] : null;
            VarCommand command = new(tokens.LineNumber, tokens.OriginalLine, tokens.Arguments[0], tokens.Arguments[1],
                                     tokens.Arguments[2], defaultValue);
            commands.Add(command);
        }
    }
}
