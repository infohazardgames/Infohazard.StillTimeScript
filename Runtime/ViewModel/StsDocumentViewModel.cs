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
        private StsCursorPos? _cursorPosition;
        private StsCursorRange? _selection;
        private int _rememberedCursorColumn;
        private float _scrollValue;

        private static readonly string[] LineSeparators = { "\r\n", "\n", "\r" };

        public IReadOnlyList<string> ScriptLines => _scriptLines;

        public bool IsModified { get; private set; }

        public bool CursorActive { get; private set; }

        public StsCursorPos CursorPosition {
            get => _cursorPosition ?? StsCursorPos.Zero;
            set {
                if (value.Column < 0 || value.Line < 0 || value.Line >= _scriptLines.Count) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_cursorPosition == value) return;
                _rememberedCursorColumn = value.Column;
                _cursorPosition = new StsCursorPos(value.Line, Math.Min(value.Column, _scriptLines[value.Line].Length));
                CursorActive = true;
                CursorChanged?.Invoke(CursorPosition);
            }
        }

        public int CursorLine {
            get => _cursorPosition?.Line ?? 0;
            set => CursorPosition = new StsCursorPos(value, _rememberedCursorColumn);
        }

        public int CursorColumn {
            get => _cursorPosition?.Column ?? 0;
            set => CursorPosition = new StsCursorPos(CursorPosition.Line, value);
        }

        public bool SelectionActive { get; private set; }

        public StsCursorPos SelectionStart {
            get => _selection?.Start ?? StsCursorPos.Zero;
            set => Selection = new StsCursorRange(value, SelectionEnd);
        }

        public StsCursorPos SelectionEnd {
            get => _selection?.End ?? StsCursorPos.Zero;
            set => Selection = new StsCursorRange(SelectionStart, value);
        }

        public StsCursorRange Selection {
            get => _selection ?? StsCursorRange.Empty;
            set {
                if (!IsValidCursorPosition(value.Start) || !IsValidCursorPosition(value.End)) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value == _selection) return;

                _selection = value;
                SelectionActive = true;
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
        public event Action<StsCursorPos>? CursorChanged;
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

        public void ClearSelection() {
            _selection = null;
            SelectionActive = false;
            SelectionChanged?.Invoke(StsCursorRange.Empty);
        }

        private bool IsValidCursorPosition(StsCursorPos value) {
            return value.Line >= 0 &&
                   value.Line < _scriptLines.Count &&
                   value.Column >= 0 &&
                   value.Column <= _scriptLines[value.Line].Length;
        }

        public string? GetSelectedText() {
            return GetText(Selection);
        }

        public string? GetText(StsCursorRange range) {
            if (!SelectionActive) return null;

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
