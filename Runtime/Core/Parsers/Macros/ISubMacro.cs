using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public interface ISubMacro {
        public IEnumerable<string> Expand(LineTokens callTokens);
        
        public IEnumerable<CommandToken> EnumerateTokens() => Enumerable.Empty<CommandToken>();
    }
}
