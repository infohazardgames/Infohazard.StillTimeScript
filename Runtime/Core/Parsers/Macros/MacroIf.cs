#nullable enable

using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class MacroIf : ISubMacro {
        private readonly MacroParameters _parameters;
        private readonly List<string> _conditions;
        private readonly List<ISubMacro> _ifSection;

        public MacroIf(MacroParameters parameters, List<string> conditions, List<ISubMacro> ifSection) {
            _parameters = parameters;
            _conditions = conditions;
            _ifSection = ifSection;
        }

        public bool CheckCondition(LineTokens callTokens) {
            foreach (string condition in _conditions) {
                string? value = _parameters.GetParameterValue(condition, callTokens);
                if (string.IsNullOrEmpty(value) || (bool.TryParse(value, out bool result) && !result)) {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<string> Expand(LineTokens callTokens) {
            return _ifSection.SelectMany(s => s.Expand(callTokens));
        }
    }
}
