#nullable enable

using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Annotations;
using UnityEngine;

namespace Infohazard.StillTimeScript.ViewModel {
    public class StsDocumentViewModel : IDisposable {
        private readonly List<string> _scriptLines;
        private List<List<LineAnnotation>?>? _annotations;
        private bool _needsToUpdateAnnotations;
        private int _deferralCount;
        private Vector2Int? _cursorPosition;
        private Vector2Int? _selectionStart;
        private Vector2Int? _selectionEnd;
        private int _rememberedCursorX;
        private float _scrollValue;

        public IReadOnlyList<string> ScriptLines => _scriptLines;

        public bool IsModified { get; private set; }

        public bool CursorActive { get; private set; }

        public Vector2Int CursorPosition {
            get => _cursorPosition ?? Vector2Int.zero;
            set {
                if (value.x < 0 || value.y < 0 || value.y >= _scriptLines.Count) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_cursorPosition == value) return;
                _rememberedCursorX = value.x;
                _cursorPosition = new Vector2Int(Math.Min(value.x, _scriptLines[value.y].Length), value.y);
                CursorActive = true;
                CursorChanged?.Invoke(CursorPosition);
            }
        }

        public int CursorX {
            get => _cursorPosition?.x ?? 0;
            set => CursorPosition = new Vector2Int(value, CursorPosition.y);
        }

        public int CursorY {
            get => _cursorPosition?.y ?? 0;
            set => CursorPosition = new Vector2Int(_rememberedCursorX, value);
        }

        public bool SelectionActive { get; private set; }

        public Vector2Int SelectionStart {
            get => _selectionStart ?? Vector2Int.zero;
            set {
                if (!IsValidCursorPosition(value)) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_selectionStart == value) return;
                _selectionStart = value;
                _selectionEnd ??= value;
                SelectionActive = true;
                SelectionChanged?.Invoke(_selectionStart.Value, _selectionEnd.Value);
            }
        }

        public Vector2Int SelectionEnd {
            get => _selectionEnd ?? Vector2Int.zero;
            set {
                if (!IsValidCursorPosition(value)) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_selectionEnd == value) return;
                _selectionEnd = value;
                _selectionStart ??= value;
                SelectionActive = true;
                SelectionChanged?.Invoke(_selectionStart.Value, _selectionEnd.Value);
            }
        }

        public bool SelectionReverse {
            get {
                if (!_selectionStart.HasValue || !_selectionEnd.HasValue) return false;

                return SelectionStart.y == SelectionEnd.y
                    ? SelectionStart.x > SelectionEnd.x
                    : SelectionStart.y > SelectionEnd.y;
            }
        }

        public Vector2Int SelectionMin => SelectionReverse ? SelectionEnd : SelectionStart;
        public Vector2Int SelectionMax => SelectionReverse ? SelectionStart : SelectionEnd;

        public Vector2Int EndPosition => ScriptLines.Count == 0
            ? Vector2Int.zero
            : new Vector2Int(ScriptLines[^1].Length, ScriptLines.Count - 1);

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
        public event Action<Vector2Int, Vector2Int>? SelectionChanged;
        public event Action<Vector2Int>? CursorChanged;
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

        public void DeleteText(Vector2Int minPosition, Vector2Int maxPosition) {
            if (minPosition.y < 0 || minPosition.y >= _scriptLines.Count ||
                maxPosition.y < 0 || maxPosition.y > _scriptLines.Count)
                return;

            if (minPosition.x < 0 || minPosition.x > _scriptLines[minPosition.y].Length ||
                maxPosition.x < 0 || maxPosition.x > _scriptLines[maxPosition.y].Length)
                return;

            if (minPosition.y > maxPosition.y || (minPosition.y == maxPosition.y && minPosition.x >= maxPosition.x))
                return;

            string keptContentOnFirstLine = _scriptLines[minPosition.y][..minPosition.x];
            string keptContentOnLastLine = maxPosition.y < _scriptLines.Count
                ? _scriptLines[maxPosition.y][maxPosition.x..]
                : string.Empty;

            if (maxPosition.y > minPosition.y) {
                _scriptLines.RemoveRange(minPosition.y + 1, maxPosition.y - minPosition.y);
                _annotations?.RemoveRange(minPosition.y + 1, maxPosition.y - minPosition.y);
                LinesRemoved?.Invoke(new StsRange(minPosition.y + 1, maxPosition.y - minPosition.y));
            }

            _scriptLines[minPosition.y] = keptContentOnFirstLine + keptContentOnLastLine;
            if (_annotations != null) _annotations[minPosition.y] = null;
            LinesModified?.Invoke(new StsRange(minPosition.y, 1));

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();
        }

        public void InsertNewLine(Vector2Int position) {
            if (position.y < 0 || position.y >= _scriptLines.Count)
                return;

            if (position.x < 0 || position.x > _scriptLines[position.y].Length)
                return;

            string curLine = _scriptLines[position.y][..position.x];
            string nextLine = _scriptLines[position.y][position.x..];

            _scriptLines[position.y] = curLine;
            if (_annotations != null) _annotations[position.y] = null;
            LinesModified?.Invoke(new StsRange(position.y, 1));

            _scriptLines.Insert(position.y + 1, nextLine);
            _annotations?.Insert(position.y + 1, null);
            LinesInserted?.Invoke(new StsRange(position.y + 1, 1));

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();
        }

        public void InsertText(Vector2Int position, string text) {
            if (position.y < 0 || position.y >= _scriptLines.Count)
                return;

            if (position.x < 0 || position.x > _scriptLines[position.y].Length)
                return;

            string beforeCursor = _scriptLines[position.y][..position.x];
            string afterCursor = _scriptLines[position.y][position.x..];

            _scriptLines[position.y] = beforeCursor + text + afterCursor;
            if (_annotations != null) _annotations[position.y] = null;
            LinesModified?.Invoke(new StsRange(position.y, 1));

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();
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

        public void SetSelectionRange(Vector2Int start, Vector2Int end) {
            if (!IsValidCursorPosition(start)) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (!IsValidCursorPosition(end)) {
                throw new ArgumentOutOfRangeException(nameof(end));
            }

            if (_selectionStart == start && _selectionEnd == end) return;

            _selectionStart = start;
            _selectionEnd = end;
            SelectionActive = true;
            SelectionChanged?.Invoke(_selectionStart.Value, _selectionEnd.Value);
        }

        public void ClearSelection() {
            _selectionStart = null;
            _selectionEnd = null;
            SelectionActive = false;
            SelectionChanged?.Invoke(Vector2Int.zero, Vector2Int.zero);
        }

        private bool IsValidCursorPosition(Vector2Int value) {
            return value.y >= 0 &&
                   value.y < _scriptLines.Count &&
                   value.x >= 0 &&
                   value.x <= _scriptLines[value.y].Length;
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
