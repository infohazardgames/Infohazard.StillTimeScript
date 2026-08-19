using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Annotations;

namespace Infohazard.StillTimeScript.ViewModel {
    public static class Annotator {
        public static List<List<LineAnnotation>> Annotate(IReadOnlyList<string> lines) {
            List<List<LineAnnotation>> annotations = new();

            bool isContinuedText = false;
            for (int i = 0; i < lines.Count; i++) {
                string line = lines[i];
                List<LineAnnotation> lineAnnotations = new();
                annotations.Add(lineAnnotations);

                StsRange actualRange =
                    Tokenizer.GetActualRangeFromLine(line, new StsRange(0, line.Length), out StsRange? commentRange);

                if (commentRange.HasValue) {
                    lineAnnotations.Add(new CommentAnnotation(i, commentRange.Value));
                }

                if (actualRange.Length <= 0) continue;

                if (isContinuedText) {
                    Token token = Tokenizer.ReadTextToEnd(line, actualRange, out isContinuedText);
                    lineAnnotations.Add(new StringAnnotation(i, token.Range));
                    continue;
                }

                try {
                    LineTokens lineTokens = Tokenizer.Tokenize(i, line, actualRange, out isContinuedText);

                    if (CommandParserDelegator.IsCommand(lineTokens.Command.Text)) {
                        lineAnnotations.Add(new KeywordAnnotation(i, lineTokens.Command.Range));
                    }

                    if (lineTokens.Text != null) {
                        lineAnnotations.Add(new StringAnnotation(i, lineTokens.Text.Value.Range));
                    }

                } catch (ParsingException ex) {
                    lineAnnotations.Add(new ErrorAnnotation(i, new StsRange(0, line.Length), ex.Message));
                }
            }

            return annotations;
        }
    }
}
