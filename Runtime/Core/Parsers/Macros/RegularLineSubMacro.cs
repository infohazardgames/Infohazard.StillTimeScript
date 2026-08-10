using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class RegularLineSubMacro : ISubMacro {
        private readonly MacroParameters _parameters;
        private readonly string _macroLine;

        public RegularLineSubMacro(MacroParameters parameters, string macroLine) {
            _parameters = parameters;
            _macroLine = macroLine;
        }

        public IEnumerable<string> Expand(LineTokens callTokens) {
            yield return _parameters.EvaluateMacroLine(callTokens, _macroLine);
        }
    }
}
