#nullable enable

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers.Macros {
    public class MacroParameters {
        private static readonly Regex InterpRegex = new(@"\$[0-9a-zA-Z_]+");

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
            int paramIndex = _normalParameters.FindIndex(p => p.Name == paramName);
            if (paramIndex >= 0) {
                return _normalParameters[paramIndex];
            }

            paramIndex = _optionalParameters.FindIndex(p => p.Name == paramName);
            if (paramIndex >= 0) {
                return _optionalParameters[paramIndex];
            }

            if (_varArgsParameter.HasValue && _varArgsParameter.Value.Name == paramName) {
                return _varArgsParameter;
            }

            if (_textParameter.HasValue && _textParameter.Value.Name == paramName) {
                return _textParameter;
            }

            return null;
        }

        public string? GetParameterValue(string paramName, LineTokens tokens) {
            int paramIndex = _normalParameters.FindIndex(p => p.Name == paramName);
            if (paramIndex >= 0) {
                return tokens.Arguments[paramIndex];
            }

            paramIndex = _optionalParameters.FindIndex(p => p.Name == paramName);
            if (paramIndex >= 0) {
                int indexInArgs = _normalParameters.Count + paramIndex;
                if (indexInArgs < tokens.Arguments.Length) {
                    return tokens.Arguments[indexInArgs];
                } else {
                    return _optionalParameters[paramIndex].DefaultValue;
                }
            }

            if (_varArgsParameter.HasValue && _varArgsParameter.Value.Name == paramName) {
                int varArgStartIndex = _normalParameters.Count + _optionalParameters.Count;
                if (varArgStartIndex < tokens.Arguments.Length) {
                    return string.Join(" ", tokens.Arguments[varArgStartIndex..]);
                } else {
                    return null;
                }
            }

            if (_textParameter.HasValue && _textParameter.Value.Name == paramName) {
                return tokens.Text;
            }

            throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                       $"Unrecognized macro parameter '{paramName}'");
        }

        public void ValidateMacroLine(int lineNumber, string macroLine) {
            Match match = InterpRegex.Match(macroLine);
            while (match.Success) {
                string paramName = match.Value[1..];
                if (GetMacroParameter(paramName) == null) {
                    throw new ParsingException(lineNumber, macroLine, $"Unrecognized macro parameter '{paramName}'");
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
                _textParameter is { DefaultValue: null },
                _textParameter is { DefaultValue: not null });
        }

        public string EvaluateMacroLine(LineTokens callTokens, string macroLine) {
            string result = macroLine;

            Match match = InterpRegex.Match(macroLine);
            while (match.Success) {
                string paramName = match.Value[1..];
                string? value = GetParameterValue(paramName, callTokens);
                result = result.Replace(match.Value, value ?? string.Empty);
                match = match.NextMatch();
            }

            return result;
        }
    }

    public struct MacroParameter {
        public string Name { get; }
        public MacroParameterType Type { get; }
        public string? DefaultValue { get; }

        public MacroParameter(string name, MacroParameterType type, string? defaultValue = null) {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    public enum MacroParameterType {
        Regular,
        VarArg,
        Text,
    }
}
