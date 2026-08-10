using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("choice")]
    public class ChoiceCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 1, 2, true);
            ChoiceCommand choiceCommand = new(
                tokens.LineNumber,
                tokens.OriginalLine,
                tokens.Text,
                tokens.Arguments[0],
                tokens.Arguments.Length > 1 ? tokens.Arguments[1] : null);

            commands.Add(choiceCommand);
        }
    }
}
