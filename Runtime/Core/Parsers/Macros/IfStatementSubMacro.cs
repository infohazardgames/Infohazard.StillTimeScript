using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class IfStatementSubMacro : ISubMacro {
        private readonly MacroIf _ifSection;
        private readonly List<MacroIf> _elseIfs;
        private readonly List<ISubMacro> _elseSection;

        public IfStatementSubMacro(MacroIf ifSection, List<MacroIf> elseIfs, List<ISubMacro> elseSection) {
            _ifSection = ifSection;
            _elseIfs = elseIfs;
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
    }
}
