using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("speaker")]
    public class SpeakerCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 2, 2, true);
            if (!StsColor.TryParseHex(tokens.Arguments[1], out StsColor color)) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Invalid color value {tokens.Arguments[1]}");
            }

            SpeakerCommand command = new(tokens.LineNumber, tokens.OriginalLine, tokens.Arguments[0], color,
                                         tokens.Text);
            commands.Add(command);
        }
    }
}
