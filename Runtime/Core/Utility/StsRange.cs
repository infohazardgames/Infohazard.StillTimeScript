using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct StsRange {
        public int Start { get; set; }
        public int Length { get; set; }

        public int End {
            get => Start + Length;
            set => Length = value - Start;
        }

        public int Min {
            get => Start;
            set {
                int e = End;
                Start = value;
                End = e;
            }
        }

        public StsRange(int start, int length) {
            Start = start;
            Length = Math.Max(0, length);
        }

        public static StsRange FromStartEnd(int start, int end) {
            return new StsRange {
                Start = start,
                End = end,
            };
        }

        public static StsRange FromLength(int length) {
            return new StsRange {
                Start = 0,
                Length = length,
            };
        }

        public static StsRange FromRange(Range range, int refLength) {
            (int offset, int length) = range.GetOffsetAndLength(refLength);
            return new StsRange(offset, length);
        }

        public StsRange Trim(ReadOnlySpan<char> text) {
            int start = Start;
            int end = End;

            while (start < end && char.IsWhiteSpace(text[start])) {
                start++;
            }

            while (end > start && char.IsWhiteSpace(text[end - 1])) {
                end--;
            }

            return FromStartEnd(start, end);
        }

        public override string ToString() {
            return $"[{Start}, {End})";
        }
    }
}
