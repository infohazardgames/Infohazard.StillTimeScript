using System;
using System.Collections.Generic;
using System.Text;

namespace Infohazard.StillTimeScript.Core.Utility {
    public static class Tokenizer {
        public static bool IsValidCommandNameCharacter(char c) => c == '_' || char.IsLetterOrDigit(c);

        public static bool IsValidCommandName(ReadOnlySpan<char> s) {
            for (int i = 0; i < s.Length; i++) {
                if (!IsValidCommandNameCharacter(s[i])) return false;
            }

            return true;
        }

        public static LineTokens TokenizeAndAdvance(ParsingState parsingState) {
            int lineNumber = parsingState.LineNumber;
            ParsingState.LineInfo line = parsingState.MoveNext();
            LineTokens tokens = Tokenize(lineNumber, line.Line, line.RangeInLine, out bool isContinued);

            if (isContinued) {
                Token text = tokens.GetRequiredText();
                ReadContinuingText(parsingState, ref text);
                tokens = new LineTokens(tokens.LineNumber, tokens.OriginalLine, tokens.Command, tokens.Arguments, text);
            }

            return tokens;
        }

        public static LineTokens Tokenize(int lineNumber, string line, StsRange range, out bool isTextContinued) {
            StsRange actualRange = GetActualRangeFromLine(line, range, out _);

            if (actualRange.Length == 0) {
                throw new Exception("Unexpected state");
            }

            int cmdEnd = actualRange.Start;
            for (int i = actualRange.Start; i < actualRange.End; i++) {
                char c = line[i];
                if (IsValidCommandNameCharacter(c) || c == '!') {
                    cmdEnd++;
                } else {
                    break;
                }
            }

            if (cmdEnd == actualRange.Start) {
                throw new ParsingException(lineNumber, line, "Failed to parse command name");
            }

            Token cmd = Token.FromRangeInSource(actualRange.Start..cmdEnd, line);
            Token[] args = null;

            StsRange remaining = StsRange.FromStartEnd(cmdEnd, actualRange.End).Trim(line);
            if (remaining.Length == 0) {
                isTextContinued = false;
                return new LineTokens(lineNumber, line, cmd, null, null);
            }

            if (line[remaining.Start] == '(') {
                int argsEnd = remaining.Start;
                List<Token> argList = TokenizeArgumentList(lineNumber, line, ref argsEnd, remaining.End);
                args = argList.Count > 0 ? argList.ToArray() : null;
                remaining = StsRange.FromStartEnd(argsEnd, actualRange.End).Trim(line);
            }

            if (remaining.Length == 0) {
                isTextContinued = false;
                return new LineTokens(lineNumber, line, cmd, args, null);
            }

            if (line[remaining.Start] != ':') {
                throw new ParsingException(lineNumber, line, "Expected ':' before text");
            }

            remaining.Min++;
            Token text = ReadTextToEnd(line, remaining, out isTextContinued);
            return new LineTokens(lineNumber, line, cmd, args, text);
        }

        public static Token TokenizeCommandName(int lineNumber, string line, StsRange range) {
            StsRange actualRange = GetActualRangeFromLine(line, range, out _);

            int cmdEnd = actualRange.Start;
            while (cmdEnd < actualRange.End) {
                char curChar = line[cmdEnd];
                if (!IsValidCommandNameCharacter(curChar) && curChar != '!') break;
                cmdEnd++;
            }

            if (cmdEnd == 0) {
                throw new ParsingException(lineNumber, line, "Failed to tokenize command name");
            }

            return Token.FromRangeInSource(actualRange.Start..cmdEnd, line);
        }

        public static StsRange GetActualRangeFromLine(string line, StsRange range, out StsRange? commentRange) {
            int commentIndex = line.IndexOf("//", range.Start, StringComparison.Ordinal);

            StsRange actualRange;
            if (commentIndex >= 0 && commentIndex < range.End) {
                actualRange = StsRange.FromStartEnd(range.Start, commentIndex);
                commentRange = StsRange.FromStartEnd(commentIndex, line.Length);
            } else {
                actualRange = range;
                commentRange = null;
            }

            return actualRange.Trim(line);
        }

        private static void ReadContinuingText(ParsingState state, ref Token text) {
            StringBuilder result = new();
            bool isTextContinued = true;

            while (isTextContinued && !state.IsEnded) {
                ParsingState.LineInfo textLine = state.MoveNext();
                StsRange actualRange = GetActualRangeFromLine(textLine.Line, textLine.RangeInLine, out _);
                if (actualRange.Length <= 0) continue;

                result.Append(" ");
                result.Append(ReadTextToEnd(textLine.Line, actualRange, out isTextContinued).Text);
            }

            if (result.Length > 0) {
                text.Text += result.ToString();
            }
        }

        public static Token ReadTextToEnd(string line, StsRange range, out bool isContinued) {
            ReadOnlySpan<char> lineToEnd = line[..range.End].TrimEnd();
            isContinued = lineToEnd.EndsWith("\\");
            if (isContinued) {
                lineToEnd = lineToEnd[..^1];
            }

            int index = range.Start;
            SkipWhitespace(lineToEnd, ref index, null);
            return new Token(new StsRange(index, lineToEnd.Length - index), lineToEnd[index..].ToString());
        }

        public static void ValidateTokens(
            in LineTokens tokens,
            int minArgs,
            int maxArgs,
            bool requireText,
            bool optionalText = false) {
            int argCount = tokens.Arguments?.Length ?? 0;
            if (argCount < minArgs || argCount > maxArgs) {
                throw new ParsingException(
                    tokens.LineNumber,
                    tokens.OriginalLine,
                    $"Unexpected arg count for command {tokens.Command} - expected between {minArgs} and {maxArgs}");
            }

            if (!requireText && !optionalText && tokens.Text != null) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Unexpected text for command {tokens.Command}");
            } else if (requireText && tokens.Text == null) {
                throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                           $"Missing expected text for command {tokens.Command}");
            }
        }

        public static List<Token> TokenizeArgumentList(
            int lineNumber,
            string line,
            ref int index,
            int? end) {

            SkipWhitespace(line, ref index, end);
            EnsureNotAtEnd(lineNumber, line, index, end);

            if (line[index] != '(') {
                throw new ParsingException(lineNumber, line, $"Expected '(' at index {index}");
            }

            index++;

            SkipWhitespace(line, ref index, end);

            List<Token> result = new();
            while (EnsureNotAtEnd(lineNumber, line, index, end) && line[index] != ')') {
                Token argument = TokenizeArgument(lineNumber, line, ref index, end);
                result.Add(argument);
                SkipWhitespace(line, ref index, end);
                EnsureNotAtEnd(lineNumber, line, index, end);
                if (line[index] != ',') continue;

                index++;
                SkipWhitespace(line, ref index, end);
            }

            index++;

            return result;
        }

        private static Token TokenizeArgument(int lineNumber, string line, ref int index, int? end) {
            int openCount = 0;

            end ??= line.Length;
            int argEnd;
            EnsureNotAtEnd(lineNumber, line, index, end);
            for (argEnd = index; argEnd < end.Value; argEnd++) {
                char c = line[argEnd];
                if (c == '"') {
                    argEnd = GetEndOfStringLiteral(lineNumber, line, argEnd, end) - 1;
                } else if (c == '(') {
                    openCount++;
                } else if (c == ')') {
                    if (openCount > 0) {
                        openCount--;
                    } else {
                        break;
                    }
                } else if (c == ',' && openCount == 0) {
                    break;
                }
            }

            Token result = Token.FromRangeInSource(index..argEnd, line);
            index = argEnd;
            return result;
        }

        public static void SkipWhitespace(ReadOnlySpan<char> span, ref int index, int? end) {
            int max = end ?? span.Length;

            while (index < max && char.IsWhiteSpace(span[index])) {
                index++;
            }
        }

        public static bool EnsureNotAtEnd(int lineNumber, string line, int index, int? end) {
            if (index >= line.Length || index >= end) {
                throw new ParsingException(lineNumber, line, "Unexpected end of line");
            }

            return true;
        }

        public static int GetEndOfStringLiteral(int lineNumber, string line, Index start, Index? end) {
            bool isEscape = false;
            int endOffset = end?.GetOffset(line.Length) ?? line.Length;
            for (int i = start.GetOffset(line.Length) + 1; i < endOffset; i++) {
                char c = line[i];

                if (isEscape) {
                    isEscape = false;
                } else if (c == '\\') {
                    isEscape = true;
                } else if (c == '"') {
                    return i + 1;
                }
            }

            throw new ParsingException(lineNumber, line, "Encountered non-closed string literal.");
        }

        public static Index Add(this Index index, int value) {
            if (index.IsFromEnd) {
                return new Index(index.Value - value);
            } else {
                return new Index(index.Value + value);
            }
        }
    }
}
