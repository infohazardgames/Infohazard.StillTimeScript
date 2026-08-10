using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("pop")]
    [CustomCommandParser("trypop")]
    public class PopCommandParser : ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 0, 0, false);
            commands.Add(new PopCommand(tokens.LineNumber, tokens.OriginalLine, tokens.Command == "trypop"));
        }
    }
}
