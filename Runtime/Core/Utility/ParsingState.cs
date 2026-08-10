#nullable enable

using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Parsers.Macros;

namespace Infohazard.StillTimeScript.Core.Utility {
    public class ParsingState {
        public readonly Dictionary<string, Macro> Macros;

        private readonly string[] _lines;
        private int _lineNumber;
        private readonly List<BufferedLine> _reverseBufferedLines;

        public bool IsEnded => _reverseBufferedLines.Count == 0 && _lineNumber >= _lines.Length;

        public int LineNumber => _reverseBufferedLines.Count > 0 ? _reverseBufferedLines[^1].LineNumber : _lineNumber;

        public int Version { get; private set; } = 0;

        public string? CurrentLine => _reverseBufferedLines.Count > 0
            ? _reverseBufferedLines[^1].Line
            : _lineNumber < _lines.Length
                ? _lines[_lineNumber]
                : null;

        public ParsingState(string[] lines, int lineNumber) {
            _lines = lines;
            _lineNumber = lineNumber;
            _reverseBufferedLines = new List<BufferedLine>();
            Macros = new Dictionary<string, Macro>();
        }

        public string? MoveNext() {
            string? current = CurrentLine;

            if (_reverseBufferedLines.Count > 0) {
                _reverseBufferedLines.RemoveAt(_reverseBufferedLines.Count - 1);
            } else if (_lineNumber < _lines.Length) {
                _lineNumber++;
            }
            
            Version++;

            return current;
        }

        public void Prepend(int lineNumber, string line) {
            _reverseBufferedLines.Add(new BufferedLine(lineNumber, line));
            Version++;
        }

        public void PrependRange(IReadOnlyList<BufferedLine> lines) {
            for (int i = lines.Count - 1; i >= 0; i--) {
                _reverseBufferedLines.Add(lines[i]);
            }
            
            Version++;
        }

        public struct BufferedLine {
            public string Line;
            public int LineNumber;

            public BufferedLine(int lineNumber, string line) {
                LineNumber = lineNumber;
                Line = line;
            }
        }
    }
}
