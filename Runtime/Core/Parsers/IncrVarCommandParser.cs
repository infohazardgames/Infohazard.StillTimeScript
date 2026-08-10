using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("incr")]
    public class IncrVarCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 2, 2, false);
            decimal incrValue = decimal.TryParse(tokens.Arguments[1], out decimal t)
                ? t
                : throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                             $"Invalid decimal value {tokens.Arguments[1]}");

            IncrVarCommand command = new(tokens.LineNumber, tokens.OriginalLine, tokens.Arguments[0], incrValue);
            commands.Add(command);
        }
    }
}
