#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class MacroParameters {
        public static readonly Regex InterpRegex = new(@"\$[0-9a-zA-Z_]+");

        private readonly List<MacroParameter> _normalParameters;
        private readonly List<MacroParameter> _optionalParameters;
        private readonly MacroParameter? _varArgsParameter;
        private readonly MacroParameter? _textParameter;

        public MacroParameters(
            List<MacroParameter> normalParameters,
            List<MacroParameter> optionalParameters,
            MacroParameter? varArgsParameter,
            MacroParameter? textParameter) {
            _normalParameters = normalParameters;
            _optionalParameters = optionalParameters;
            _varArgsParameter = varArgsParameter;
            _textParameter = textParameter;
        }

        public MacroParameter? GetMacroParameter(string paramName) {
            int paramIndex = _normalParameters.FindIndex(p => p.Name.Text == paramName);
            if (paramIndex >= 0) {
                return _normalParameters[paramIndex];
            }

            paramIndex = _optionalParameters.FindIndex(p => p.Name.Text == paramName);
            if (paramIndex >= 0) {
                return _optionalParameters[paramIndex];
            }

            if (_varArgsParameter.HasValue && _varArgsParameter.Value.Name.Text == paramName) {
                return _varArgsParameter;
            }

            if (_textParameter.HasValue && _textParameter.Value.Name.Text == paramName) {
                return _textParameter;
            }

            return null;
        }

        public string? GetParameterValue(string paramName, LineTokens tokens) {
            int paramIndex = _normalParameters.FindIndex(p => p.Name.Text == paramName);
            if (paramIndex >= 0) {
                return tokens.Arguments[paramIndex].Text;
            }

            paramIndex = _optionalParameters.FindIndex(p => p.Name.Text == paramName);
            if (paramIndex >= 0) {
                int indexInArgs = _normalParameters.Count + paramIndex;
                if (indexInArgs < tokens.Arguments.Length) {
                    return tokens.Arguments[indexInArgs].Text;
                } else {
                    return _optionalParameters[paramIndex].DefaultValue?.Text;
                }
            }

            if (_varArgsParameter.HasValue && _varArgsParameter.Value.Name.Text == paramName) {
                int varArgStartIndex = _normalParameters.Count + _optionalParameters.Count;
                if (varArgStartIndex < tokens.Arguments.Length) {
                    return string.Join(" ", tokens.Arguments[varArgStartIndex..].Select(t => t.Text));
                } else {
                    return null;
                }
            }

            if (_textParameter.HasValue && _textParameter.Value.Name.Text == paramName) {
                return tokens.Text?.Text;
            }

            throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                       $"Unrecognized macro parameter '{paramName}'");
        }

        public void ValidateMacroLine(ParsingState.LineInfo line) {
            Match match = InterpRegex.Match(line.Line, line.RangeInLine.Start, line.RangeInLine.Length);
            while (match.Success) {
                string paramName = match.Value[1..];
                if (GetMacroParameter(paramName) == null) {
                    throw new ParsingException(line.LineNumber, line.Line, $"Unrecognized macro parameter '{paramName}'");
                }

                match = match.NextMatch();
            }
        }

        public void ValidateTokens(LineTokens callTokens) {
            int minArgCount = _normalParameters.Count;
            int maxArgCount = _normalParameters.Count + _optionalParameters.Count;
            if (_varArgsParameter.HasValue) {
                maxArgCount = 1000;
            }

            Tokenizer.ValidateTokens(
                callTokens,
                minArgCount,
                maxArgCount,
                _textParameter is { IsOptional: false },
                _textParameter is { IsOptional: true });
        }

        public string EvaluateMacroLine(LineTokens callTokens, string macroLine, StsRange range) {
            string result = macroLine;

            Match match = InterpRegex.Match(macroLine, range.Start, range.Length);
            while (match.Success) {
                string paramName = match.Value[1..];
                string? value = GetParameterValue(paramName, callTokens);
                result = result.Replace(match.Value, value ?? string.Empty);
                match = match.NextMatch();
            }

            return result;
        }
        
        public IEnumerable<CommandToken> EnumerateTokens() {
            foreach (MacroParameter normalParameter in _normalParameters) {
                yield return new CommandToken(normalParameter.Name, CommandTokenType.Definition);
            }

            foreach (MacroParameter optionalParameter in _optionalParameters) {
                yield return new CommandToken(optionalParameter.Name, CommandTokenType.Definition);
            }

            if (_varArgsParameter.HasValue) {
                yield return new CommandToken(_varArgsParameter.Value.Name, CommandTokenType.Definition);
            }
            
            if (_textParameter.HasValue) {
                yield return new CommandToken(_textParameter.Value.Name, CommandTokenType.Definition);
            }
        }
    }

    public struct MacroParameter {
        public Token Name { get; }
        public MacroParameterType Type { get; }
        public Token? DefaultValue { get; }
        public bool IsOptional { get; }

        public MacroParameter(Token name, MacroParameterType type, Token? defaultValue, bool isOptional) {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            IsOptional = defaultValue.HasValue || isOptional;
        }
    }

    public enum MacroParameterType {
        Regular,
        VarArg,
        Text,
    }
}
