using System;
using System.Globalization;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct StsColor : IEquatable<StsColor> {
        public float R;
        public float G;
        public float B;
        public float A;

        public float this[int index] {
            readonly get => index switch {
                0 => R,
                1 => G,
                2 => B,
                3 => A,
                _ => throw new IndexOutOfRangeException($"Index {index} must be in [0..3] range"),
            };
            set {
                switch (index) {
                    case 0:
                        R = value;
                        break;
                    case 1:
                        G = value;
                        break;
                    case 2:
                        B = value;
                        break;
                    case 3:
                        A = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} must be in [0..3] range");
                }
            }
    }

        public StsColor(float r, float g, float b, float a = 1) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public readonly string ToHexString(bool includeAlpha = false) {
            Span<char> span = stackalloc char[includeAlpha ? 8 : 6];
            ToHexString(span);
            return span.ToString();
        }

        public readonly void ToHexString(Span<char> span) {
            switch (span.Length) {
                case 3:
                    FormatComponents(span, 1, 3);
                    break;
                case 4:
                    FormatComponents(span, 1, 4);
                    break;
                case 6:
                    FormatComponents(span, 2, 3);
                    break;
                case 8:
                    FormatComponents(span, 2, 4);
                    break;
                default:
                    throw new ArgumentException(
                        $"Provided span has unexpected length {span.Length}. Must be 3, 4, 6, or 8.");
            }
        }

        public readonly void FormatComponents(Span<char> hex, int cSize, int cCount) {
            int cMax = (1 << (cSize * 4)) - 1;

            for (int i = 0; i < cCount; i++) {
                float vFloat = this[i];
                int v = (int)Math.Round(vFloat * cMax, MidpointRounding.AwayFromZero);
                int start = i * cSize;
                Span<char> c = hex.Slice(start, cSize);
                if (!v.TryFormat(c, out _, "x2"))
                    throw new Exception("Unexpected problem writing integer as hex");
            }
        }

        public static bool TryParseHex(ReadOnlySpan<char> hex, out StsColor color) {
            if (hex.StartsWith("#"))
                hex = hex[1..];

            color = default;
            return hex.Length switch {
                3 => ParseComponents(hex, 1, 3, out color),
                4 => ParseComponents(hex, 1, 4, out color),
                6 => ParseComponents(hex, 2, 3, out color),
                8 => ParseComponents(hex, 2, 4, out color),
                _ => false,
            };
        }

        private static bool ParseComponents(ReadOnlySpan<char> hex, int cSize, int cCount, out StsColor color) {
            color = new StsColor(0, 0, 0, 1);

            float cMax = (1 << (cSize * 4)) - 1;

            for (int i = 0; i < cCount; i++) {
                int start = i * cSize;
                ReadOnlySpan<char> c = hex.Slice(start, cSize);
                if (!int.TryParse(c, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int v))
                    return false;

                float vFloat = v / cMax;
                color[i] = vFloat;
            }

            return true;
        }

        public bool Equals(StsColor other) {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
        }

        public override bool Equals(object obj) {
            return obj is StsColor other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(R, G, B, A);
        }

        public static bool operator ==(StsColor left, StsColor right) {
            return left.Equals(right);
        }

        public static bool operator !=(StsColor left, StsColor right) {
            return !left.Equals(right);
        }
    }
}
