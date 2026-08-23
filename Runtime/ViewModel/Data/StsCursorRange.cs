using System;

namespace Infohazard.StillTimeScript.ViewModel.Data {
    public readonly struct StsCursorRange : IEquatable<StsCursorRange> {
        public StsCursorPos Start { get; }

        public StsCursorPos End { get; }

        public bool IsReverse => Start > End;

        public StsCursorPos Min => IsReverse ? End : Start;

        public StsCursorPos Max => IsReverse ? Start : End;
        
        public bool IsEmpty => Start == End;

        private static readonly string[] LineSeparators = { "\r\n", "\n", "\r" };

        public static StsCursorRange Empty => new(StsCursorPos.Zero, StsCursorPos.Zero);

        public StsCursorRange(StsCursorPos start, StsCursorPos end) {
            Start = start;
            End = end;
        }

        public StsCursorRange(int line, int startColumn, int endColumn) {
            Start = new StsCursorPos(line, startColumn);
            End = new StsCursorPos(line, endColumn);
        }

        public StsCursorRange(int startLine, int startColumn, int endLine, int endColumn) {
            Start = new StsCursorPos(startLine, startColumn);
            End = new StsCursorPos(endLine, endColumn);
        }

        public static StsCursorRange GetRangeOfText(StsCursorPos start, ReadOnlySpan<char> text) {
            int newLineCount = 0;
            int lengthOfLastLine = 0;
            for (int i = 0; i < text.Length; i++) {
                foreach (string lineSeparator in LineSeparators) {
                    ReadOnlySpan<char> remaining = text[i..];
                    if (!remaining.StartsWith(lineSeparator))  continue;
                    newLineCount++;
                    i += lineSeparator.Length - 1;
                    lengthOfLastLine = remaining.Length - lineSeparator.Length;
                    break;
                }
            }

            StsCursorPos end;
            if (newLineCount == 0) {
                end = new StsCursorPos(start.Line, start.Column + text.Length);
            } else {
                end = new StsCursorPos(start.Line + newLineCount, lengthOfLastLine);
            }

            return new StsCursorRange(start, end);
        }

        public bool Equals(StsCursorRange other) {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj) {
            return obj is StsCursorRange other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(StsCursorRange left, StsCursorRange right) {
            return left.Equals(right);
        }

        public static bool operator !=(StsCursorRange left, StsCursorRange right) {
            return !left.Equals(right);
        }
    }
}
