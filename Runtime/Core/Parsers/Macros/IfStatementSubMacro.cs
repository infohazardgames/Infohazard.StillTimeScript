using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class IfStatementSubMacro : ISubMacro {
        public Token? ElseToken { get; }
        
        private readonly MacroIf _ifSection;
        private readonly List<MacroIf> _elseIfs;
        private readonly List<ISubMacro> _elseSection;

        public IfStatementSubMacro(MacroIf ifSection, List<MacroIf> elseIfs, Token? elseToken, 
                                   List<ISubMacro> elseSection) {
            _ifSection = ifSection;
            _elseIfs = elseIfs;
            ElseToken = elseToken;
            _elseSection = elseSection;
        }

        public IEnumerable<string> Expand(LineTokens callTokens) {
            if (_ifSection.CheckCondition(callTokens)) {
                return _ifSection.Expand(callTokens);
            }

            foreach (MacroIf elseIf in _elseIfs) {
                if (elseIf.CheckCondition(callTokens)) {
                    return elseIf.Expand(callTokens);
                }
            }

            return _elseSection.SelectMany(s => s.Expand(callTokens));
        }

        public IEnumerable<CommandToken> EnumerateTokens() {
            foreach (MacroIf macroIf in _elseIfs.Prepend(_ifSection)) {
                foreach (CommandToken token in macroIf.EnumerateTokens()) {
                    yield return token;
                }
            }
            
            if (ElseToken != null) {
                yield return new CommandToken(ElseToken.Value, CommandTokenType.Keyword);
            }
            
            foreach (ISubMacro subMacro in _elseSection) {
                foreach (CommandToken token in subMacro.EnumerateTokens()) {
                    yield return token;
                }
            }
        }
    }
}
