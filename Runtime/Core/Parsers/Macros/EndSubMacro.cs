using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class EndSubMacro : ISubMacro {
        public Token Token { get; }
        
        public EndSubMacro(Token token) {
            Token = token;
        }
        
        public IEnumerable<string> Expand(LineTokens callTokens) => Enumerable.Empty<string>();

        public IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(Token, CommandTokenType.Keyword);
        }
    }
}