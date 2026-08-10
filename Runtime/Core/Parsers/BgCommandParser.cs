using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("bg")]
    public class BgCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 1, 2, false);

            if (!StsColor.TryParseHex(tokens.Arguments[0], out StsColor bgColor)) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Invalid color value {tokens.Arguments[0]}");
            }

            float bgTime = 0;
            if (tokens.Arguments.Length > 1 && !float.TryParse(tokens.Arguments[1], out bgTime)) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Invalid float value {tokens.Arguments[1]}");
            }

            BgCommand command = new(tokens.LineNumber, tokens.OriginalLine,bgColor, bgTime);
            commands.Add(command);
        }
    }
}
