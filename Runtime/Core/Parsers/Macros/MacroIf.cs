#nullable enable

using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class MacroIf : ISubMacro {
        private readonly MacroParameters _parameters;
        private readonly List<Token> _conditions;
        private readonly List<ISubMacro> _ifSection;
        
        public Token IfToken { get; }

        public MacroIf(Token ifToken, MacroParameters parameters, List<Token> conditions, List<ISubMacro> ifSection) {
            _parameters = parameters;
            _conditions = conditions;
            _ifSection = ifSection;
            IfToken = ifToken;
        }

        public bool CheckCondition(LineTokens callTokens) {
            foreach (Token condition in _conditions) {
                string? value = _parameters.GetParameterValue(condition.Text, callTokens);
                if (string.IsNullOrEmpty(value) || (bool.TryParse(value, out bool result) && !result)) {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<string> Expand(LineTokens callTokens) {
            return _ifSection.SelectMany(s => s.Expand(callTokens));
        }

        public IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(IfToken, CommandTokenType.Keyword);
            foreach (Token condition in _conditions) {
                yield return new CommandToken(condition, CommandTokenType.Reference);
            }

            foreach (ISubMacro subMacro in _ifSection) {
                foreach (CommandToken token in subMacro.EnumerateTokens()) {
                    yield return token;
                }
            }
        }
    }
}
