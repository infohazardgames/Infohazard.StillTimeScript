using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("delay")]
    public class DelayCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 1, 1, false);

            if (!float.TryParse(tokens.Arguments[0], out float delayTime)) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Invalid float value {tokens.Arguments[1]}");
            }

            commands.Add(new DelayCommand(tokens.LineNumber, tokens.OriginalLine, delayTime));
        }
    }
}
