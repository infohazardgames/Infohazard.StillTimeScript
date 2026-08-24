using System.Collections.Generic;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class RegularLineSubMacro : ISubMacro {
        private readonly MacroParameters _parameters;
        private readonly int _lineNumber;
        private readonly string _macroLine;

        public RegularLineSubMacro(MacroParameters parameters, int lineNumber, string macroLine) {
            _parameters = parameters;
            _lineNumber = lineNumber;
            _macroLine = macroLine;
        }

        public IEnumerable<string> Expand(LineTokens callTokens) {
            yield return _parameters.EvaluateMacroLine(callTokens, _macroLine, new StsRange(0, _macroLine.Length));
        }

        public IEnumerable<CommandToken> EnumerateTokens() {
            LineTokens tokens;
            try {
                tokens = Tokenizer.Tokenize(_lineNumber, _macroLine, new StsRange(0, _macroLine.Length), out _);
            } catch {
                yield break;
            }

            if (CommandParserDelegator.IsCommand(tokens.Command.Text)) {
                yield return new CommandToken(tokens.Command, CommandTokenType.Keyword);
            }
            
            Match match = MacroParameters.InterpRegex.Match(_macroLine);
            while (match.Success) {
                string paramName = match.Value[1..];
                MacroParameter? parameter = _parameters.GetMacroParameter(paramName);
                if (parameter.HasValue) {
                    StsRange range = new(match.Index + 1, match.Length - 1);
                    yield return new CommandToken(new Token(_lineNumber, range, paramName), CommandTokenType.Reference);
                }

                match = match.NextMatch();
            }
        }
    }
}
