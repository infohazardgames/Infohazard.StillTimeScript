#nullable enable

using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Parsers.Macros;

namespace Infohazard.StillTimeScript.Core.Utility {
    public class ParsingState {
        public readonly Dictionary<string, Macro> Macros;

        private readonly string[] _lines;
        private int _lineNumber;
        private readonly List<LineInfo> _reverseBufferedLines;

        public bool IsEnded => _reverseBufferedLines.Count == 0 && _lineNumber >= _lines.Length;

        public int LineNumber => _reverseBufferedLines.Count > 0 ? _reverseBufferedLines[^1].LineNumber : _lineNumber;

        public int Version { get; private set; } = 0;

        public LineInfo CurrentLine => _reverseBufferedLines.Count > 0
            ? _reverseBufferedLines[^1]
            : _lineNumber < _lines.Length
                ? new LineInfo(_lineNumber, _lines[_lineNumber])
                : throw new InvalidOperationException("ParsingState is ended, cannot get current line");

        public ParsingState(string[] lines, int lineNumber) {
            _lines = lines;
            _lineNumber = lineNumber;
            _reverseBufferedLines = new List<LineInfo>();
            Macros = new Dictionary<string, Macro>();
        }

        public LineInfo MoveNext() {
            LineInfo current = CurrentLine;

            if (_reverseBufferedLines.Count > 0) {
                _reverseBufferedLines.RemoveAt(_reverseBufferedLines.Count - 1);
            } else if (_lineNumber < _lines.Length) {
                _lineNumber++;
            }

            Version++;

            return current;
        }

        public void Prepend(int lineNumber, string line, StsRange rangeInLine) {
            _reverseBufferedLines.Add(new LineInfo(lineNumber, line, rangeInLine));
            Version++;
        }

        public void PrependRange(IReadOnlyList<LineInfo> lines) {
            for (int i = lines.Count - 1; i >= 0; i--) {
                _reverseBufferedLines.Add(lines[i]);
            }

            Version++;
        }

        public struct LineInfo {
            public int LineNumber;
            public string Line;
            public StsRange RangeInLine;

            public ReadOnlySpan<char> Span => Line.AsSpan(RangeInLine.Start, RangeInLine.Length);

            public LineInfo(int lineNumber, string line, StsRange rangeInLine) {
                LineNumber = lineNumber;
                Line = line;
                RangeInLine = rangeInLine;
            }

            public LineInfo(int lineNumber, string line) : this(lineNumber, line, new StsRange(0, line.Length)) { }
        }
    }
}
