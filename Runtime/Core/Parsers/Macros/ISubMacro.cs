using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public interface ISubMacro {
        public IEnumerable<string> Expand(LineTokens callTokens);
    }
}
