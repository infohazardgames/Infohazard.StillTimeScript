#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Annotations;
using Infohazard.StillTimeScript.ViewModel.Data;
using UnityEngine;

namespace Infohazard.StillTimeScript.ViewModel {
    public class StsDocumentViewModel : IDisposable {
        private readonly List<string> _scriptLines;
        private List<List<LineAnnotation>?>? _annotations;
        private bool _needsToUpdateAnnotations;
        private int _deferralCount;
        private StsCursorRange _selection;
        private float _scrollValue;
        private static readonly string[] LineSeparators = { "\r\n", "\n", "\r" };

        public IReadOnlyList<string> ScriptLines => _scriptLines;

        public bool IsModified { get; private set; }

        public bool CursorActive { get; private set; }
        
        public int RememberedCursorColumn { get; set; }

        public StsCursorPos CursorPosition {
            get => _selection.End;
            set => Selection = new StsCursorRange(value, value);
        }

        public int CursorLine {
            get => CursorPosition.Line;
            set => CursorPosition = new StsCursorPos(value, RememberedCursorColumn);
        }

        public int CursorColumn {
            get => CursorPosition.Column;
            set => CursorPosition = new StsCursorPos(CursorPosition.Line, value);
        }

        public bool SelectionActive => CursorActive && !Selection.IsEmpty;

        public StsCursorPos SelectionStart {
            get => _selection.Start;
            set => Selection =
                new StsCursorRange(value, new StsCursorPos(_selection.End.Line, RememberedCursorColumn));
        }

        public int SelectionStartLine {
            get => SelectionStart.Line;
            set => SelectionStart = new StsCursorPos(value, SelectionStart.Column);
        }

        public int SelectionStartColumn {
            get => SelectionStart.Column;
            set => SelectionStart = new StsCursorPos(SelectionStart.Line, value);
        }

        public StsCursorPos SelectionEnd {
            get => _selection.End;
            set => Selection = new StsCursorRange(SelectionStart, value);
        }
        
        public int SelectionEndLine {
            get => SelectionEnd.Line;
            set => SelectionEnd = new StsCursorPos(value, RememberedCursorColumn);
        }
        
        public int SelectionEndColumn {
            get => SelectionEnd.Column;
            set => SelectionEnd = new StsCursorPos(SelectionEnd.Line, value);
        }

        public StsCursorRange Selection {
            get => _selection;
            set {
                if (!IsValidCursorPosition(value.Start)) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                
                if (value.End.Column < 0 || value.End.Line < 0 || value.End.Line >= _scriptLines.Count) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value == _selection && CursorActive) return;
                
                RememberedCursorColumn = value.End.Column;
                StsCursorPos endPos = new(value.End.Line,
                    Math.Min(value.End.Column, _scriptLines[value.End.Line].Length));
                
                _selection = new StsCursorRange(value.Start, endPos);
                CursorActive = true;
                SelectionChanged?.Invoke(value);
            }
        }

        public StsCursorPos EndPosition => ScriptLines.Count == 0
            ? StsCursorPos.Zero
            : new StsCursorPos(ScriptLines.Count - 1, ScriptLines[^1].Length);

        public float ScrollValue {
            get => _scrollValue;
            set {
                if (_scrollValue == value) return;
                _scrollValue = value;
                ScrollChanged?.Invoke(_scrollValue);
            }
        }

        public event Action<bool>? IsModifiedChanged;
        public event Action<StsRange>? LinesInserted;
        public event Action<StsRange>? LinesRemoved;
        public event Action<StsRange>? LinesModified;
        public event Action<StsCursorRange>? SelectionChanged;
        public event Action<float>? ScrollChanged;

        public StsDocumentViewModel(IEnumerable<string> scriptLines) {
            _scriptLines = new List<string>(scriptLines);
            UpdateAnnotations();
        }

        public void Dispose() { }

        public void Rebuild(IEnumerable<string> lines) {
            int count = _scriptLines.Count;
            _scriptLines.Clear();
            _annotations?.Clear();
            LinesRemoved?.Invoke(new StsRange(0, count));

            _scriptLines.AddRange(lines);
            LinesInserted?.Invoke(new StsRange(0, _scriptLines.Count));
            UpdateAnnotations();

            IsModified = true;
            IsModifiedChanged?.Invoke(true);
        }

        public void DeleteText(StsCursorRange range) {
            StsCursorPos min = range.Min;
            StsCursorPos max = range.Max;

            if (min.Line < 0 || min.Line >= _scriptLines.Count ||
                max.Line < 0 || max.Line > _scriptLines.Count)
                return;

            if (min.Column < 0 || min.Column > _scriptLines[min.Line].Length ||
                max.Column < 0 || max.Column > _scriptLines[max.Line].Length)
                return;

            string keptContentOnFirstLine = _scriptLines[min.Line][..min.Column];
            string keptContentOnLastLine = max.Line < _scriptLines.Count
                ? _scriptLines[max.Line][max.Column..]
                : string.Empty;

            if (max.Line > min.Line) {
                _scriptLines.RemoveRange(min.Line + 1, max.Line - min.Line);
                _annotations?.RemoveRange(min.Line + 1, max.Line - min.Line);
                LinesRemoved?.Invoke(new StsRange(min.Line + 1, max.Line - min.Line));
            }

            _scriptLines[min.Line] = keptContentOnFirstLine + keptContentOnLastLine;
            if (_annotations != null) _annotations[min.Line] = null;
            LinesModified?.Invoke(new StsRange(min.Line, 1));

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();
        }

        public StsCursorPos InsertText(StsCursorPos position, string text) {
            string beforeCursor = _scriptLines[position.Line][..position.Column];
            string afterCursor = _scriptLines[position.Line][position.Column..];

            string[] linesToInsert = text.Split(LineSeparators, StringSplitOptions.None);

            int endY = position.Line + linesToInsert.Length;
            int lastY = endY - 1;

            _scriptLines[position.Line] = beforeCursor + linesToInsert[0];
            _scriptLines.InsertRange(position.Line + 1, linesToInsert[1..]);
            StsCursorPos insertEnd = new(lastY, _scriptLines[lastY].Length);
            _scriptLines[lastY] += afterCursor;

            if (_annotations != null) {
                _annotations[position.Line] = null;
                if (linesToInsert.Length > 1) {
                    _annotations.InsertRange(
                        position.Line + 1, Enumerable.Repeat<List<LineAnnotation>?>(null, linesToInsert.Length - 1));
                }
            }

            LinesModified?.Invoke(new StsRange(position.Line, 1));
            if (endY > position.Line) {
                LinesInserted?.Invoke(StsRange.FromStartEnd(position.Line + 1, endY + 1));
            }

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();

            return insertEnd;
        }

        public void ClearModified() {
            IsModified = false;
            IsModifiedChanged?.Invoke(IsModified);
        }

        private void UpdateAnnotations() {
            if (_deferralCount > 0) {
                _needsToUpdateAnnotations = true;
            } else {
                _annotations = Annotator.Annotate(_scriptLines);
                LinesModified?.Invoke(new StsRange(0, _scriptLines.Count));
            }
        }

        public List<LineAnnotation>? GetAnnotations(int lineIndex) {
            if (_annotations == null) return null;
            if (lineIndex < 0 || lineIndex >= _annotations.Count) {
                throw new ArgumentOutOfRangeException(nameof(lineIndex));
            }

            return _annotations[lineIndex];
        }

        public EventDeferral DeferUpdates() {
            return new EventDeferral(this);
        }

        public void ClearCursor() {
            _selection = StsCursorRange.Empty;
            CursorActive = false;
            SelectionChanged?.Invoke(StsCursorRange.Empty);
        }

        private bool IsValidCursorPosition(StsCursorPos value) {
            return value.Line >= 0 &&
                   value.Line < _scriptLines.Count &&
                   value.Column >= 0 &&
                   value.Column <= _scriptLines[value.Line].Length;
        }

        public void OffsetCursorColumn(int colDiff, bool moveOnlyEnd) {
            (int line, int col) = CursorPosition;

            int moveAmt = Math.Abs(colDiff);
            int moveDir = Math.Sign(colDiff);
            for (int i = 0; i < moveAmt; i++) {
                if (moveDir < 0) {
                    if (col > 0) {
                        col--;
                    } else if (line > 0) {
                        line--;
                        col = _scriptLines[line].Length;
                    } else {
                        break;
                    }
                } else {
                    if (col < _scriptLines[line].Length) {
                        col++;
                    } else if (line < _scriptLines.Count - 1) {
                        line++;
                        col = 0;
                    } else {
                        break;
                    }
                }
            }

            if (moveOnlyEnd) {
                SelectionEnd = new StsCursorPos(line, col);
            } else {
                CursorPosition = new StsCursorPos(line, col);
            }
        }
        
        public void OffsetCursorLine(int lineDiff, bool moveOnlyEnd) {
            int line = CursorLine;
            int col = RememberedCursorColumn;
            
            int moveAmt = Math.Abs(lineDiff);
            int moveDir = Math.Sign(lineDiff);
            for (int i = 0; i < moveAmt; i++) {
                if (moveDir < 0) {
                    if (line > 0) {
                        line--;
                    } else if (col > 0) {
                        col = 0;
                    } else {
                        break;
                    }
                } else {
                    if (line < _scriptLines.Count - 1) {
                        line++;
                    } else if (col < _scriptLines[line].Length) {
                        col = _scriptLines[line].Length;
                    } else {
                        break;
                    }
                }
            }
            
            if (moveOnlyEnd) {
                SelectionEnd = new StsCursorPos(line, col);
            } else {
                CursorPosition = new StsCursorPos(line, col);
            }
        }

        public void SetCursorColumn(int value, bool moveOnlyEnd) {
            if (moveOnlyEnd) {
                SelectionEndColumn = value;
            } else {
                CursorColumn = value;
            }
        }
        
        public void SetCursorLine(int value, bool moveOnlyEnd) {
            if (moveOnlyEnd) {
                SelectionEndLine = value;
            } else {
                CursorLine = value;
            }
        }
        
        public void SetCursorPosition(StsCursorPos value, bool moveOnlyEnd) {
            if (moveOnlyEnd) {
                SelectionEnd = value;
            } else {
                CursorPosition = value;
            }
        }

        public string? GetSelectedText() {
            return GetText(Selection);
        }

        public string? GetText(StsCursorRange range) {
            if (range.IsEmpty) return null;

            StsCursorPos min = range.Min;
            StsCursorPos max = range.Max;

            if (min.Line == max.Line) {
                return _scriptLines[min.Line][min.Column..max.Column];
            }

            StringBuilder builder = new();
            builder.Append(_scriptLines[min.Line][min.Column..]).Append("\n");
            for (int i = min.Line + 1; i < max.Line; i++) {
                builder.Append(_scriptLines[i]).Append("\n");
            }

            builder.Append(_scriptLines[max.Line][..max.Column]);
            return builder.ToString();
        }

        public readonly struct EventDeferral : IDisposable {
            private readonly StsDocumentViewModel _viewModel;

            public EventDeferral(StsDocumentViewModel viewModel) {
                _viewModel = viewModel;
                _viewModel._deferralCount++;
            }

            public void Dispose() {
                _viewModel._deferralCount--;
                if (_viewModel is { _deferralCount: 0, _needsToUpdateAnnotations: true }) {
                    _viewModel.UpdateAnnotations();
                }
            }
        }
    }
}
