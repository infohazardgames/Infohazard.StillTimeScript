#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Annotations;

namespace Infohazard.StillTimeScript.ViewModel {
    public static class Annotator {
        public static List<List<LineAnnotation>?> Annotate(IReadOnlyList<string> lines) {
            List<List<LineAnnotation>?> annotations = new();

            ParsingState parsingState = new(lines.ToArray(), 0);

            List<ICommand> commands = new();
            while (!parsingState.IsEnded) {
                ParsingState.LineInfo lineInfo = parsingState.CurrentLine;
                string line = lineInfo.Line;
                int lineNumber = parsingState.LineNumber;

                if (annotations.Count <= lineNumber) {
                    annotations.AddRange(
                        Enumerable.Repeat<List<LineAnnotation>?>(null, lineNumber + 1 - annotations.Count));
                }

                List<LineAnnotation> lineAnnotations = annotations[lineNumber] ??= new List<LineAnnotation>();

                StsRange actualRange =
                    Tokenizer.GetActualRangeFromLine(line, lineInfo.RangeInLine, out StsRange? commentRange);

                if (commentRange.HasValue) {
                    lineAnnotations.Add(new CommentAnnotation(commentRange.Value));
                }

                if (actualRange.Length == 0) {
                    parsingState.MoveNext();
                    continue;
                }

                try {
                    LineTokens lineTokens = Tokenizer.Tokenize(lineNumber, line, actualRange, out _);
                    if (CommandParserDelegator.IsCommand(lineTokens.Command.Text)) {
                        lineAnnotations.Add(new KeywordAnnotation(lineTokens.Command.Range));
                    }
                } catch (Exception ex) {
                    lineAnnotations.Add(new ErrorAnnotation(actualRange, ex.Message));
                    parsingState.MoveNext();

                    continue;
                }

                int version = parsingState.Version;
                try {
                    CommandParserDelegator.ParseLine(parsingState, commands, false);
                } catch (Exception ex) {
                    lineAnnotations.Add(new ErrorAnnotation(actualRange, ex.Message));

                    if (parsingState.Version == version) {
                        parsingState.MoveNext();
                    }
                }
            }

            GraphData graphData = GraphData.Empty();

            foreach (IResourceCommand resourceCommand in commands.OfType<IResourceCommand>()) {
                try {
                    resourceCommand.CreateResources(graphData);
                } catch (Exception) {
                    // Not able to handle exception here yet.
                }
            }

            foreach (ICommand command in commands) {
                List<LineAnnotation> lineAnnotations = annotations[command.LineNumber] ??= new List<LineAnnotation>();
                HandleCommandTokens(command, graphData, lineAnnotations, command.EnumerateTokens().ToList());
            }

            return annotations;
        }

        private static void HandleCommandTokens(
            ICommand command,
            GraphData graphData,
            List<LineAnnotation> lineAnnotations,
            List<CommandToken> commandTokens) {
            List<CommandToken> tempList = new();
            foreach (CommandToken token in commandTokens) {
                switch (token.Type) {
                    case CommandTokenType.Expression:
                        try {
                            tempList.Clear();
                            ExpressionParser.ParseExpression(command, graphData, command.Line,
                                                             token.Token.Range, token.RequiredValueType, tempList);

                            HandleCommandTokens(command, graphData, lineAnnotations, tempList);
                        } catch (Exception ex) {
                            lineAnnotations.Add(new ErrorAnnotation(token.Token.Range, ex.Message));
                        }

                        break;
                    case CommandTokenType.StringExpression:
                        try {
                            tempList.Clear();
                            ExpressionParser.ParseStringExpression(command, graphData, command.Line,
                                                                   token.Token.Range, tempList);

                            HandleCommandTokens(command, graphData, lineAnnotations, tempList);
                        } catch (Exception ex) {
                            lineAnnotations.Add(new ErrorAnnotation(token.Token.Range, ex.Message));
                        }

                        break;
                    case CommandTokenType.ResourceReference:
                        if (!graphData.Resources.ContainsKey(token.Token.Text)) {
                            lineAnnotations.Add(new ErrorAnnotation(token.Token.Range, "Resource not found"));
                        } else {
                            lineAnnotations.Add(new DefinitionReferenceAnnotation(token.Token.Range));
                        }

                        break;
                    case CommandTokenType.NodeReference:
                        if (!graphData.Nodes.ContainsKey(token.Token.Text)) {
                            lineAnnotations.Add(new ErrorAnnotation(token.Token.Range, "Label not found"));
                        } else {
                            lineAnnotations.Add(new DefinitionReferenceAnnotation(token.Token.Range));
                        }

                        break;
                    case CommandTokenType.ColorLiteral:
                        lineAnnotations.Add(
                            new ColorLiteralAnnotation(
                                token.Token.Range, StsColor.TryParseHex(token.Token.Text, out StsColor color)
                                    ? color
                                    : default));
                        break;
                    case CommandTokenType.StringLiteral:
                        lineAnnotations.Add(new StringAnnotation(token.Token.Range));
                        break;
                    case CommandTokenType.Definition:
                        lineAnnotations.Add(new DefinitionAnnotation(token.Token.Range));
                        break;
                    case CommandTokenType.Keyword:
                        lineAnnotations.Add(new KeywordAnnotation(token.Token.Range));
                        break;
                    case CommandTokenType.MacroCall:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
