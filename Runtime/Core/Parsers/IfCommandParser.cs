using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("if")]
    public class IfCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 1, 1, false, true);
            IfCommand command = new(tokens.LineNumber, tokens.OriginalLine, tokens.Arguments[0]);
            commands.Add(command);

            if (!string.IsNullOrEmpty(tokens.Text)) {
                state.Prepend(tokens.LineNumber, tokens.Text);
            }
        }
    }
}
