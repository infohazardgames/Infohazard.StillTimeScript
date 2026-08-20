#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    public static class ExpressionParser {
        private const string VisitedFunction = "visited";
        private const string CompareExchangeFunction = "compex";

        private static readonly string[][] BinaryOperatorPrecedence = {
            new[] { "^" },
            new[] { "*", "/", "%" },
            new[] { "+", "-" },
            new[] { "<", ">", "<=", ">=" },
            new[] { "==", "!=" },
            new[] { "xor" },
            new[] { "and" },
            new[] { "or" },
        };

        private static readonly Dictionary<string, BinaryMathOperator> BinaryMathOperators = new() {
            { "^", BinaryMathOperator.Power },
            { "*", BinaryMathOperator.Multiply },
            { "/", BinaryMathOperator.Divide },
            { "%", BinaryMathOperator.Modulo },
            { "+", BinaryMathOperator.Add },
            { "-", BinaryMathOperator.Subtract },
        };

        private static readonly Dictionary<string, NumberComparisonOperator> NumberComparisonOperators = new() {
            { "<", NumberComparisonOperator.Less },
            { "<=", NumberComparisonOperator.LessOrEqual },
            { ">", NumberComparisonOperator.Greater },
            { ">=", NumberComparisonOperator.GreaterOrEqual },
        };

        private static readonly Dictionary<string, EqualityOperator> EqualityOperators = new() {
            { "==", EqualityOperator.Equal },
            { "!=", EqualityOperator.NotEqual },
        };

        private static readonly Dictionary<string, BinaryLogicOperator> BinaryLogicOperators = new() {
            { "xor", BinaryLogicOperator.Xor },
            { "and", BinaryLogicOperator.And },
            { "or", BinaryLogicOperator.Or },
        };

        private static readonly string[] FunctionOperators = {
            VisitedFunction,
            CompareExchangeFunction,
        };

        private static readonly Dictionary<string, int> BinaryOperatorPrecedenceMap;
        private static readonly List<string> BinaryOperatorParsingPriority;
        private static readonly Regex StringInterpRegex = new(@"\{([^}]*)\}");

        static ExpressionParser() {
            BinaryOperatorPrecedenceMap = new Dictionary<string, int>();
            BinaryOperatorParsingPriority = new List<string>();

            for (int i = 0; i < BinaryOperatorPrecedence.Length; i++) {
                string[] group = BinaryOperatorPrecedence[i];
                foreach (string op in group) {
                    BinaryOperatorPrecedenceMap[op] = i;
                    BinaryOperatorParsingPriority.Add(op);
                }
            }

            // Order by length descending
            BinaryOperatorParsingPriority.Sort((a, b) => b.Length - a.Length);
        }

        public static IExpression ParseStringExpression(ICommand command,
                                                        GraphData graphData,
                                                        string line,
                                                        StsRange range,
                                                        List<CommandToken>? tokens = null) {
            StringConcatExpression expression = new();

            MatchCollection collection = StringInterpRegex.Matches(line, range.Start);

            int curIndex = range.Start;
            foreach (Match match in collection) {
                if (match.Index + match.Length > range.End) {
                    break;
                }

                if (match.Index - curIndex > 0) {
                    string strLit = EscapeString(command, line[curIndex..match.Index]);
                    expression.AddExpression(new ConstantExpression(new StsValue(strLit)));
                    tokens?.Add(new CommandToken(new Token(new StsRange(curIndex, match.Index - curIndex), strLit),
                                                 CommandTokenType.StringLiteral));
                }

                Group? group = match.Groups.Count > 1 ? match.Groups[1] : null;
                StsRange? innerRange = group != null ? new StsRange(group.Index, group.Length) : null;

                if (innerRange is { Length: > 0 }) {
                    expression.AddExpression(ParseExpression(command, graphData, line, innerRange.Value,
                                                             StsValueType.None, tokens));
                }

                curIndex = match.Index + match.Length;
            }

            if (curIndex < range.End) {
                string strLit = EscapeString(command, line[curIndex..range.End]);
                expression.AddExpression(new ConstantExpression(new StsValue(strLit)));
                tokens?.Add(new CommandToken(new Token(new StsRange(curIndex, range.End - curIndex), strLit),
                                              CommandTokenType.StringLiteral));
            }

            return expression;
        }

        private static string EscapeString(ICommand command, ReadOnlySpan<char> span) {
            StringBuilder builder = new();

            bool isEscape = false;
            for (int i = 0; i < span.Length; i++) {
                char c = span[i];

                if (isEscape) {
                    builder.Append(c switch {
                        '\\' or '"' => c,
                        'n' => '\n',
                        't' => '\t',
                        _ => throw new ParsingException(command.LineNumber, command.Line,
                                                        $"Invalid escape character '\\{c}'."),
                    });
                } else if (c == '\\') {
                    isEscape = true;
                } else {
                    builder.Append(c);
                }
            }

            if (isEscape) {
                throw new ParsingException(command.LineNumber, command.Line, "Invalid escape character '\\'.");
            }

            return builder.ToString();
        }

        public static IExpression ParseExpression(ICommand command, GraphData graphData, string line, StsRange range,
                                                  StsValueType requiredType = StsValueType.None,
                                                  List<CommandToken>? tokens = null) {
            int index = range.Start;
            IExpression expression = ParseExpression(
                command,
                graphData,
                line,
                ref index,
                ReadOnlySpan<char>.Empty,
                range.End,
                tokens);

            Tokenizer.SkipWhitespace(line, ref index, range.End);
            if (index < range.End)
                throw new ParsingException(command.LineNumber, command.Line,
                                           $"Unexpected character '{line[index]}'");

            if (requiredType != StsValueType.None && expression.Type != requiredType) {
                throw new ParsingException(command.LineNumber, command.Line,
                                           $"Expected expression of type {requiredType} but found {expression.Type}");
            }

            return expression;
        }

        private static IExpression ParseExpression(
            ICommand command,
            GraphData graphData,
            string line,
            ref int index,
            ReadOnlySpan<char> endChars,
            int? end,
            List<CommandToken>? tokens) {
            IExpression result = ReadBinaryOperatorsWithEarlierPrecedence(
                command,
                graphData,
                line,
                ref index,
                endChars,
                BinaryOperatorPrecedence.Length,
                end,
                tokens);

            if (endChars.IsEmpty) return result;

            Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, index, end);
            if (!endChars.Contains(line.AsSpan(index, 1), StringComparison.Ordinal))
                throw new Exception("Unexpected end of line");

            index++;
            return result;
        }

        private static IExpression ReadBinaryOperatorsWithEarlierPrecedence(
            ICommand command,
            GraphData graphData,
            string line,
            ref int index,
            ReadOnlySpan<char> endChars,
            int precedence,
            int? end,
            List<CommandToken>? tokens) {
            end ??= line.Length;
            IExpression currentOperand = ReadPrimaryAndUnaryOperators(command, graphData, line, ref index, end, tokens);

            while (true) {
                Tokenizer.SkipWhitespace(line, ref index, end);
                if (endChars.IsEmpty && index >= end) break;
                Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, index, end);
                if (endChars.Contains(line.AsSpan(index, 1), StringComparison.Ordinal)) break;

                string? op = null;
                foreach (string s in BinaryOperatorParsingPriority) {
                    if (!line.AsSpan(index, end.Value - index).StartsWith(s)) continue;
                    op = s;
                    break;
                }

                if (op == null) {
                    throw new ParsingException(
                        command.LineNumber,
                        command.Line,
                        $"Encountered unexpected character(s) while parsing expression: {line[index..]}");
                }

                int operatorPrecedence = BinaryOperatorPrecedenceMap[op];
                if (operatorPrecedence >= precedence) break;

                index += op.Length;
                Tokenizer.SkipWhitespace(line, ref index, end);
                Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, index, end);
                IExpression nextOperand =
                    ReadBinaryOperatorsWithEarlierPrecedence(
                        command,
                        graphData,
                        line,
                        ref index,
                        endChars,
                        operatorPrecedence,
                        end,
                        tokens);

                if (BinaryMathOperators.TryGetValue(op, out BinaryMathOperator binaryMathOperator)) {
                    currentOperand =
                        new BinaryMathExpression(currentOperand, nextOperand, binaryMathOperator);
                } else if (NumberComparisonOperators.TryGetValue(op,
                                                                 out NumberComparisonOperator
                                                                     numberComparisonOperator)) {
                    currentOperand =
                        new NumberCompareExpression(currentOperand, nextOperand, numberComparisonOperator);
                } else if (EqualityOperators.TryGetValue(op, out EqualityOperator equalityOperator)) {
                    currentOperand =
                        new EqualityExpression(currentOperand, nextOperand, equalityOperator);
                } else if (BinaryLogicOperators.TryGetValue(op, out BinaryLogicOperator binaryLogicOperator)) {
                    currentOperand =
                        new BinaryLogicExpression(currentOperand, nextOperand, binaryLogicOperator);
                } else {
                    throw new ParsingException(command.LineNumber, command.Line, $"Unexpected operator {op}");
                }
            }

            return currentOperand;
        }

        private static IExpression ReadPrimaryAndUnaryOperators(
            ICommand command,
            GraphData graphData,
            string line,
            ref int index,
            int? end,
            List<CommandToken>? tokens) {
            Tokenizer.SkipWhitespace(line, ref index, end);
            char c = line[index];

            end ??= line.Length;

            if (c is '!' or '-') {
                index++;
                IExpression operand = ReadPrimaryAndUnaryOperators(command, graphData, line, ref index, end, tokens);
                if (c == '!') {
                    return new UnaryLogicExpression(UnaryLogicOperator.Not, operand);
                } else {
                    return new UnaryMathExpression(UnaryMathOperator.Negate, operand);
                }
            }

            if (c == '(') {
                index++;
                return ParseExpression(command, graphData, line, ref index, ")", end, tokens);
            }

            if (c == '"') {
                int strEnd = Tokenizer.GetEndOfStringLiteral(command.LineNumber, line, index, end);
                tokens?.Add(new CommandToken(new Token(new StsRange(index, 1), "\""),
                                             CommandTokenType.StringLiteral));
                tokens?.Add(new CommandToken(new Token(new StsRange(strEnd - 1, 1), "\""),
                                             CommandTokenType.StringLiteral));
                IExpression strEx =
                    ParseStringExpression(command, graphData, line, StsRange.FromStartEnd(index + 1, strEnd - 1),
                                          tokens);
                index = strEnd;
                return strEx;
            }

            foreach (string funcOp in FunctionOperators) {
                if (!line.AsSpan(index, end.Value - index).StartsWith(funcOp)) continue;
                StsRange funcOpRange = new(index, funcOp.Length);
                index += funcOp.Length;
                List<Token> argumentTokens = Tokenizer.TokenizeArgumentList(command.LineNumber, line, ref index, end);
                List<IExpression> arguments =
                    argumentTokens.ConvertAll(arg => ParseExpression(command, graphData, line, arg.Range));

                if (funcOp == VisitedFunction) {
                    if (arguments.Count != 2) {
                        throw new ParsingException(
                            command.LineNumber,
                            command.Line,
                            $"Expected 2 arguments for 'visited' function, got {arguments.Count}");
                    }

                    return new VisitedExpression(arguments[0], arguments[1]);
                } else if (funcOp == CompareExchangeFunction) {
                    if (arguments.Count is < 1 or > 3) {
                        throw new ParsingException(
                            command.LineNumber,
                            command.Line,
                            $"Expected [1..3] arguments for 'compex' function, got {arguments.Count}");
                    }

                    if (arguments[0] is not VariableExpression varEx) {
                        throw new ParsingException(
                            command.LineNumber,
                            command.Line,
                            $"Expected first argument of 'compex' function to be a variable, got {arguments[0]}");
                    }

                    return new CompareExchangeExpression(
                        varEx.Variable,
                        arguments.Count > 1 ? arguments[1] : null,
                        arguments.Count > 2 ? arguments[2] : null);
                }

                tokens?.Add(new CommandToken(Token.FromRangeInSource(funcOpRange, line), CommandTokenType.Keyword));
            }

            int i;
            for (i = index; i < end; i++) {
                c = line[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '#' && c != '.') break;
            }

            StsRange singleItemRange = StsRange.FromStartEnd(index, i);
            index = i;
            return ParseSingleItemExpression(command, graphData, line, singleItemRange, tokens);
        }

        private static IExpression ParseSingleItemExpression(ICommand command,
                                                             GraphData graphData,
                                                             string line, StsRange range,
                                                             List<CommandToken>? tokens) {
            ReadOnlySpan<char> span = line.AsSpan(range.Start, range.Length);

            if (span.StartsWith("#") && StsColor.TryParseHex(span, out StsColor color)) {
                tokens?.Add(new CommandToken(Token.FromRangeInSource(range, line), CommandTokenType.ColorLiteral));
                return new ConstantExpression(new StsValue(color));
            } else if (decimal.TryParse(span, out decimal num)) {
                return new ConstantExpression(new StsValue(num));
            } else if (bool.TryParse(span, out bool b)) {
                tokens?.Add(new CommandToken(Token.FromRangeInSource(range, line), CommandTokenType.Keyword));
                return new ConstantExpression(new StsValue(b));
            }

            string itemStr = span.ToString();
            if (graphData.Resources.TryGetValue(itemStr, out Resource.Resource resource)) {
                tokens?.Add(new CommandToken(Token.FromRangeInSource(range, line), CommandTokenType.ResourceReference));
                if (resource is Variable variable) {
                    return new VariableExpression(variable);
                } else {
                    return new ConstantExpression(new StsValue(resource));
                }
            } else if (graphData.Nodes.TryGetValue(itemStr, out INode node)) {
                tokens?.Add(new CommandToken(Token.FromRangeInSource(range, line), CommandTokenType.NodeReference));
                return new ConstantExpression(new StsValue(node));
            } else {
                throw new ParsingException(command.LineNumber, command.Line,
                                           $"Failed to parse value at {range}: '{itemStr}'");
            }
        }
    }
}
