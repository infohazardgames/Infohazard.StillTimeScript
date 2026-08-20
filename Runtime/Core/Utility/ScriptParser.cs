using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Parsers;

namespace Infohazard.StillTimeScript.Core.Utility {
    public static class ScriptParser {
        public static List<ICommand> ParseScript(string scriptContent) {
            string[] lines = scriptContent.Split('\n');
            List<ICommand> commands = new();
            ParsingState state = new(lines, 0);

            while (!state.IsEnded) {
                CommandParserDelegator.ParseLine(state, commands, true);
            }

            return commands;
        }
    }
}
