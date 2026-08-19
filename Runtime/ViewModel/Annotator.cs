using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Annotations;

namespace Infohazard.StillTimeScript.ViewModel {
    public static class Annotator {
        public static List<List<LineAnnotation>> Annotate(IReadOnlyList<string> lines) {
            List<List<LineAnnotation>> annotations = new();

            for (int i = 0; i < lines.Count; i++) {
                string line = lines[i];
                List<LineAnnotation> lineAnnotations = new();
                annotations.Add(lineAnnotations);

                Tokenizer.GetActualRangeFromLine(line, new StsRange(0, line.Length), out StsRange? commentRange);
                if (commentRange.HasValue) {
                    lineAnnotations.Add(new CommentAnnotation(i, commentRange.Value.Start, commentRange.Value.End));
                }
            }

            return annotations;
        }
    }
}
