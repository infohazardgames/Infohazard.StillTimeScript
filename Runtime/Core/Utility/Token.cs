using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct Token {
        public StsRange Range { get; set; }
        public string Text { get; set; }

        public Token(StsRange range, string text) {
            Range = range;
            Text = text;
        }

        public static Token FromRangeInSource(Range range, ReadOnlySpan<char> sourceText) {
            return new Token {
                Range = StsRange.FromRange(range, sourceText.Length),
                Text = sourceText[range].ToString(),
            };
        }

        public override string ToString() {
            return Text;
        }
    }
}
