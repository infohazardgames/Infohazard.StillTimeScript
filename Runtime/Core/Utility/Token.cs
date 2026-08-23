using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct Token {
        public int LineNumber { get; set; }
        public StsRange Range { get; set; }
        public string Text { get; set; }

        public Token(int lineNumber, StsRange range, string text) {
            LineNumber = lineNumber;
            Range = range;
            Text = text;
        }

        public static Token FromRangeInSource(int lineNumber, Range range, ReadOnlySpan<char> sourceText) {
            return new Token {
                LineNumber = lineNumber,
                Range = StsRange.FromRange(range, sourceText.Length),
                Text = sourceText[range].ToString(),
            };
        }

        public override string ToString() {
            return Text;
        }
    }
}
