#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Parsers.Macros;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    [CustomCommandParser("macro")]
    public class MacroCommandParser : ICommandParser {
        private static readonly string[] StopBeforeCommandsForIf = { "!else", "!elif", "!end" };

        public static bool IsMacroCommand(string command) {
            return command is "!if" or "!else" or "!elif" or "!end";
        }

        public void ParseCommand(ParsingState state, List<ICommand> commands) {
            LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
            Tokenizer.ValidateTokens(tokens, 1, 1000, false, true);

            Token identifier = tokens.Arguments[0];
            MacroParameters macroParameters = ParseMacroParameters(tokens);
            List<ISubMacro> subMacros = new();

            ParseSubMacros(state, macroParameters, subMacros);

            Macro macro = new(identifier, macroParameters, subMacros);
            state.Macros.Add(identifier.Text, macro);
            
            commands.Add(new MacroCommand(tokens, macro));
        }

        private static MacroParameters ParseMacroParameters(LineTokens tokens) {
            Token[] parameters = tokens.Arguments[1..];
            List<MacroParameter> normalParams = new();
            List<MacroParameter> optionalParams = new();
            MacroParameter? varArgsParam = null;
            MacroParameter? textParam = null;

            for (int i = 0; i < parameters.Length; i++) {
                string param = parameters[i].Text;
                StsRange range = parameters[i].Range;
                StsRange paramRange;
                MacroParameterType paramType;
                StsRange? defaultValueRange;
                bool isOptional = false;

                if (param.EndsWith("...")) {
                    paramRange = range[..^3];
                    paramType = MacroParameterType.VarArg;
                    defaultValueRange = null;
                } else if (param.EndsWith("?")) {
                    paramRange = range[..^1];
                    paramType = MacroParameterType.Regular;
                    defaultValueRange = null;
                    isOptional = true;
                } else if (param.Contains('=')) {
                    int index = param.IndexOf('=');
                    paramRange = range[..index].Trim(tokens.OriginalLine);
                    paramType = MacroParameterType.Regular;
                    defaultValueRange = range[(index + 1)..];
                    isOptional = true;
                } else {
                    paramRange = range;
                    paramType = MacroParameterType.Regular;
                    defaultValueRange = null;
                }

                Token paramToken = Token.FromRangeInSource(tokens.LineNumber, paramRange, tokens.OriginalLine);
                Token? defaultValueToken = defaultValueRange == null
                    ? null
                    : Token.FromRangeInSource(tokens.LineNumber, defaultValueRange.Value, tokens.OriginalLine);
                
                if (!Tokenizer.IsValidCommandName(paramToken.Text)) {
                    throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                        $"Invalid macro parameter name '{paramRange}'");
                }

                if (paramType == MacroParameterType.VarArg) {
                    if (i == parameters.Length - 1) {
                        varArgsParam = new MacroParameter(paramToken, MacroParameterType.VarArg, defaultValueToken,
                            true);
                    } else {
                        throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                            $"VarArg parameter {paramRange} only allowed as last parameter");
                    }
                } else {
                    if (isOptional) {
                        optionalParams.Add(new MacroParameter(paramToken, MacroParameterType.Regular, defaultValueToken,
                            true));
                    } else {
                        if (optionalParams.Count == 0) {
                            normalParams.Add(new MacroParameter(paramToken, MacroParameterType.Regular, null, false));
                        } else {
                            throw new ParsingException(
                                tokens.LineNumber,
                                tokens.OriginalLine,
                                $"Non-optional parameter {paramRange} not allowed after optional parameters.");
                        }
                    }
                }
            }

            if (tokens.Text != null) {
                Token textParameterToken = tokens.Text.Value;

                bool isOptional = false;
                if (textParameterToken.Text.EndsWith('?')) {
                    textParameterToken = Token.FromRangeInSource(tokens.LineNumber, textParameterToken.Range[..^1],
                        tokens.OriginalLine);
                    isOptional = true;
                }
                
                if (Tokenizer.IsValidCommandName(textParameterToken.Text)) {
                    textParam = new MacroParameter(textParameterToken, MacroParameterType.Text, null, isOptional);
                } else {
                    throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                        $"Invalid macro parameter name '{textParameterToken.Text}'");
                }
            }

            MacroParameters macroParameters = new(normalParams, optionalParams, varArgsParam, textParam);
            return macroParameters;
        }

        private static void ParseSubMacros(
            ParsingState state,
            MacroParameters macroParameters,
            List<ISubMacro> subMacros,
            string[]? stopBeforeCommands = null) {
            while (!state.IsEnded) {
                ParsingState.LineInfo line = state.CurrentLine;
                StsRange actualRange = Tokenizer.GetActualRangeFromLine(line.Line, line.RangeInLine, out _);
                if (actualRange.Length <= 0) {
                    state.MoveNext();
                    continue;
                }

                if (stopBeforeCommands is { Length: > 0 } && line.Line[actualRange.Start] == '!') {
                    string cmdName = Tokenizer.TokenizeCommandName(state.LineNumber, line.Line, actualRange).ToString();
                    if (Array.IndexOf(stopBeforeCommands, cmdName) != -1) {
                        break;
                    }
                }

                ISubMacro subMacro = ParseSubMacro(state, actualRange, macroParameters);
                subMacros.Add(subMacro);
                if (subMacro is EndSubMacro) break;
            }
        }

        private static ISubMacro ParseSubMacro(
            ParsingState state,
            StsRange actualRange,
            MacroParameters macroParameters) {

            ParsingState.LineInfo line = state.CurrentLine;
            macroParameters.ValidateMacroLine(state.CurrentLine);

            if (line.Line[actualRange.Start] != '!') {
                line = state.MoveNext();
                return new RegularLineSubMacro(macroParameters, line.LineNumber, line.Span.ToString());
            } else {
                LineTokens subTokens = Tokenizer.TokenizeAndAdvance(state);
                switch (subTokens.Command.Text) {
                    case "!end":
                        Tokenizer.ValidateTokens(subTokens, 0, 0, false);
                        return new EndSubMacro(subTokens.Command);
                    case "!if":
                        Tokenizer.ValidateTokens(subTokens, 1, 100, false);
                        return ParseIfStatement(subTokens, state, macroParameters);
                    default:
                        throw new ParsingException(subTokens.LineNumber, subTokens.OriginalLine,
                            $"Unrecognized macro command '{subTokens.Command}'");
                }
            }
        }

        private static IfStatementSubMacro ParseIfStatement(
            LineTokens ifStartTokens,
            ParsingState state,
            MacroParameters macroParameters) {
            MacroIf ifSection = ParseIf(ifStartTokens, state, macroParameters);
            List<MacroIf> elseIfs = new();
            List<ISubMacro> elseSection = new();
            Token? elseToken = null;

            while (!state.IsEnded) {
                ParsingState.LineInfo line = state.CurrentLine;
                StsRange actualRange = Tokenizer.GetActualRangeFromLine(line.Line, line.RangeInLine, out _);
                if (actualRange.Length <= 0) {
                    state.MoveNext();
                    continue;
                }

                macroParameters.ValidateMacroLine(state.CurrentLine);
                LineTokens subTokens = Tokenizer.TokenizeAndAdvance(state);
                bool isEnd = false;
                switch (subTokens.Command.Text) {
                    case "!end":
                        Tokenizer.ValidateTokens(subTokens, 0, 0, false);
                        elseSection.Add(new EndSubMacro(subTokens.Command));
                        isEnd = true;
                        break;
                    case "!elif" when elseSection.Count == 0:
                        Tokenizer.ValidateTokens(subTokens, 1, 100, false);
                        elseIfs.Add(ParseIf(subTokens, state, macroParameters));
                        break;
                    case "!else" when elseSection.Count == 0:
                        Tokenizer.ValidateTokens(subTokens, 0, 0, false);
                        elseToken = subTokens.Command;
                        ParseSubMacros(state, macroParameters, elseSection, StopBeforeCommandsForIf);
                        break;
                    default:
                        throw new ParsingException(subTokens.LineNumber, subTokens.OriginalLine,
                            $"Unexpected macro command '{subTokens.Command}'");
                }

                if (isEnd) break;
            }

            return new IfStatementSubMacro(ifSection, elseIfs, elseToken, elseSection);
        }

        private static MacroIf ParseIf(
            LineTokens tokens,
            ParsingState state,
            MacroParameters macroParameters) {
            Token[] conditions = tokens.Arguments;
            foreach (Token condition in conditions) {
                if (macroParameters.GetMacroParameter(condition.Text) == null) {
                    throw new ParsingException(tokens.LineNumber, tokens.GetRequiredText().Text,
                        $"Unrecognized macro parameter '{condition.Text}'");
                }
            }

            List<ISubMacro> ifSection = new();
            ParseSubMacros(state, macroParameters, ifSection, StopBeforeCommandsForIf);
            return new MacroIf(tokens.Command, macroParameters, conditions.ToList(), ifSection);
        }
    }
}
