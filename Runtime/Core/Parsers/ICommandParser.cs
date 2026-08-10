using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    public interface ICommandParser {
        public void ParseCommand(ParsingState state, List<ICommand> commands);
    }
}
