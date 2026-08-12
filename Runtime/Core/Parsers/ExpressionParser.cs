using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Commands;
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

        public static IExpression ParseStringLiteral(Command command, GraphData graphData, ReadOnlySpan<char> span,
                                                     int index, out int endIndex) {

            bool isEscape = false;
            for (int i = index; i < span.Length; i++) {
                char c = span[i];

                if (isEscape) {
                    isEscape = false;
                } else if (c == '\\') {
                    isEscape = true;
                } else if (c == '"') {
                    endIndex = i + 1;
                    string str = span[index..i].ToString();
                    return ParseStringExpression(command, graphData, str);
                }
            }

            throw new ParsingException(command.LineNumber, command.Line, "Encountered non-closed string literal.");
        }

        public static IExpression ParseStringExpression(Command command, GraphData graphData, string stringExpr) {
            StringConcatExpression expression = new();

            MatchCollection collection = StringInterpRegex.Matches(stringExpr);

            int curIndex = 0;
            foreach (Match match in collection) {
                if (match.Index - curIndex > 0) {
                    expression.AddExpression(
                        new ConstantExpression(new StsValue(EscapeString(command, stringExpr[curIndex..match.Index]))));
                }

                string inner = match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;

                if (string.IsNullOrWhiteSpace(inner)) {
                    expression.AddExpression(new ConstantExpression(new StsValue(string.Empty)));
                } else {
                    expression.AddExpression(ParseExpression(command, graphData, inner));
                }

                curIndex = match.Index + match.Length;
            }

            if (curIndex < stringExpr.Length) {
                expression.AddExpression(
                    new ConstantExpression(new StsValue(EscapeString(command, stringExpr[curIndex..]))));
            }

            return expression;
        }

        private static string EscapeString(Command command, ReadOnlySpan<char> span) {
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

        public static IExpression ParseExpression(Command command, GraphData graphData, ReadOnlySpan<char> span,
                                                  StsValueType requiredType = StsValueType.None) {
            IExpression expression = ParseExpression(
                command,
                graphData,
                span,
                0,
                ReadOnlySpan<char>.Empty,
                out int endIndex);

            Tokenizer.SkipWhitespace(span, ref endIndex);
            if (endIndex < span.Length)
                throw new ParsingException(command.LineNumber, span.ToString(),
                                           $"Unexpected character '{span[endIndex]}'");

            if (requiredType != StsValueType.None && expression.Type != requiredType) {
                throw new ParsingException(command.LineNumber, command.Line,
                                           $"Expected expression of type {requiredType} but found {expression.Type}");
            }

            return expression;
        }

        private static IExpression ParseExpression(
            Command command,
            GraphData graphData,
            ReadOnlySpan<char> span,
            int startIndex,
            ReadOnlySpan<char> endChars,
            out int endIndex) {
            IExpression result = ReadBinaryOperatorsWithEarlierPrecedence(
                command,
                graphData,
                span,
                startIndex,
                endChars,
                BinaryOperatorPrecedence.Length,
                out endIndex);

            if (endChars.IsEmpty) return result;

            Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, span, endIndex);
            if (!endChars.Contains(span.Slice(endIndex, 1), StringComparison.Ordinal))
                throw new Exception("Unexpected end of line");

            endIndex++;
            return result;
        }

        private static IExpression ReadBinaryOperatorsWithEarlierPrecedence(
            Command command,
            GraphData graphData,
            ReadOnlySpan<char> span,
            int startIndex,
            ReadOnlySpan<char> endChars,
            int precedence,
            out int endIndex) {
            int index = startIndex;
            IExpression currentOperand = ReadPrimaryAndUnaryOperators(command, graphData, span, index, out index);

            while (true) {
                Tokenizer.SkipWhitespace(span, ref index);
                if (endChars.IsEmpty && index >= span.Length) break;
                Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, span, index);
                if (endChars.Contains(span.Slice(index, 1), StringComparison.Ordinal)) break;

                string op = null;
                foreach (string s in BinaryOperatorParsingPriority) {
                    if (!span[index..].StartsWith(s)) continue;
                    op = s;
                    break;
                }

                if (op == null) {
                    throw new ParsingException(
                        command.LineNumber,
                        command.Line,
                        $"Encountered unexpected character(s) while parsing expression: {span[index..].ToString()}");
                }

                int operatorPrecedence = BinaryOperatorPrecedenceMap[op];
                if (operatorPrecedence >= precedence) break;

                index += op.Length;
                Tokenizer.SkipWhitespace(span, ref index);
                Tokenizer.EnsureNotAtEnd(command.LineNumber, command.Line, span, index);
                IExpression nextOperand =
                    ReadBinaryOperatorsWithEarlierPrecedence(
                        command,
                        graphData,
                        span,
                        index,
                        endChars,
                        operatorPrecedence,
                        out index);

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

            endIndex = index;
            return currentOperand;
        }

        private static IExpression ReadPrimaryAndUnaryOperators(
            Command command,
            GraphData graphData,
            ReadOnlySpan<char> span,
            int startIndex,
            out int endIndex) {
            int index = startIndex;

            Tokenizer.SkipWhitespace(span, ref index);
            char c = span[index];

            if (c is '!' or '-') {
                index++;
                IExpression operand = ReadPrimaryAndUnaryOperators(command, graphData, span, index, out index);
                endIndex = index;
                if (c == '!') {
                    return new UnaryLogicExpression(UnaryLogicOperator.Not, operand);
                } else {
                    return new UnaryMathExpression(UnaryMathOperator.Negate, operand);
                }
            }

            if (c == '(') {
                index++;
                return ParseExpression(command, graphData, span, index, ")", out endIndex);
            }

            if (c == '"') {
                index++;
                return ParseStringLiteral(command, graphData, span, index, out endIndex);
            }

            foreach (string funcOp in FunctionOperators) {
                if (!span[index..].StartsWith(funcOp)) continue;
                index += funcOp.Length;
                List<IExpression> arguments =
                    Tokenizer.TokenizeArgumentList(command.LineNumber, command.Line, span, ref index)
                             .Select(arg => ParseExpression(command, graphData, arg))
                             .ToList();

                if (funcOp == VisitedFunction) {
                    if (arguments.Count != 2) {
                        throw new ParsingException(
                            command.LineNumber,
                            command.Line,
                            $"Expected 2 arguments for 'visited' function, got {arguments.Count}");
                    }

                    endIndex = index;
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

                    endIndex = index;
                    return new CompareExchangeExpression(
                        varEx.Variable,
                        arguments.Count > 1 ? arguments[1] : null,
                        arguments.Count > 2 ? arguments[2] : null);
                }
            }

            int end;
            for (end = index; end < span.Length; end++) {
                c = span[end];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '#' && c != '.') break;
            }

            ReadOnlySpan<char> item = span[index..end];
            endIndex = end;
            return ParseSingleItemExpression(command, graphData, item.ToString());
        }

        private static IExpression ParseSingleItemExpression(Command command, GraphData graphData,
                                                             ReadOnlySpan<char> item) {
            if (item.StartsWith("#") && StsColor.TryParseHex(item, out StsColor color)) {
                return new ConstantExpression(new StsValue(color));
            } else if (decimal.TryParse(item, out decimal num)) {
                return new ConstantExpression(new StsValue(num));
            } else if (bool.TryParse(item, out bool b)) {
                return new ConstantExpression(new StsValue(b));
            }

            string itemStr = item.ToString();
            if (graphData.Resources.TryGetValue(itemStr, out Resource.Resource resource)) {
                if (resource is Variable variable) {
                    return new VariableExpression(variable);
                } else {
                    return new ConstantExpression(new StsValue(resource));
                }
            } else if (graphData.Nodes.TryGetValue(itemStr, out INode node)) {
                return new ConstantExpression(new StsValue(node));
            } else {
                throw new ParsingException(command.LineNumber, command.Line, $"Failed to parse value: '{itemStr}'");
            }
        }
    }
}
