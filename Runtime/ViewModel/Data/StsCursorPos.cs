using System;

namespace Infohazard.StillTimeScript.ViewModel.Data {
    public readonly struct StsCursorPos : IComparable<StsCursorPos>, IEquatable<StsCursorPos> {

        public int Line { get; }
        public int Column { get; }

        public static StsCursorPos Zero => new(0, 0);

        public StsCursorPos(int line, int column) {
            Line = line;
            Column = column;
        }

        public bool Equals(StsCursorPos other) {
            return Line == other.Line && Column == other.Column;
        }

        public override bool Equals(object obj) {
            return obj is StsCursorPos other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Line, Column);
        }

        public static bool operator ==(StsCursorPos left, StsCursorPos right) {
            return left.Equals(right);
        }

        public static bool operator !=(StsCursorPos left, StsCursorPos right) {
            return !left.Equals(right);
        }

        public int CompareTo(StsCursorPos other) {
            int lineComparison = Line.CompareTo(other.Line);
            if (lineComparison != 0) return lineComparison;
            return Column.CompareTo(other.Column);
        }

        public static bool operator <(StsCursorPos left, StsCursorPos right) {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(StsCursorPos left, StsCursorPos right) {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(StsCursorPos left, StsCursorPos right) {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(StsCursorPos left, StsCursorPos right) {
            return left.CompareTo(right) >= 0;
        }
    }
}
