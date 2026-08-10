using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("say")]
    public class SayCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 0, 1, true);
            string saySpeaker = tokens.Arguments?.Length > 0 ? tokens.Arguments[0] : null;
            SayCommand sayCommand = new(tokens.LineNumber, tokens.OriginalLine, saySpeaker, tokens.Text);
            commands.Add(sayCommand);
        }
    }
}
