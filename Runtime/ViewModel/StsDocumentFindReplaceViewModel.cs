using System;
using System.Text.RegularExpressions;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel.Actions;
using Infohazard.StillTimeScript.ViewModel.Data;

namespace Infohazard.StillTimeScript.ViewModel {
    public class StsDocumentFindReplaceViewModel {
        private readonly StsDocumentViewModel _document;
        private readonly ActionStack _actionStack;
        private string _findText;
        private bool _occurrencesDirty;
        private bool _occurrenceIndexDirty;
        private int _occurrenceIndex;
        private StsCursorRange[] _occurrences;

        public event Action Updated;

        public string FindText {
            get => _findText;
            set {
                if (_findText == value) return;

                _findText = value;
                _occurrencesDirty = true;
                _occurrenceIndexDirty = true;

                Updated?.Invoke();
            }
        }

        public string ReplaceText { get; set; }

        public int OccurrenceIndex {
            get {
                UpdateIfNeeded();
                return _occurrenceIndex;
            }
        }

        public StsCursorRange[] Occurrences {
            get {
                UpdateIfNeeded();
                return _occurrences;
            }
        }

        public StsDocumentFindReplaceViewModel(StsDocumentViewModel document, ActionStack actionStack) {
            _document = document;
            _actionStack = actionStack;

            _document.LinesInserted += HandleLinesChanged;
            _document.LinesRemoved += HandleLinesChanged;
            _document.LinesModified += HandleLinesChanged;
            _document.SelectionChanged += HandleSelectionChanged;
        }

        public void DoFindNext() {
            UpdateIfNeeded();
            if (_occurrences.Length == 0) return;

            StsCursorRange range = _occurrences[_occurrenceIndex];
            StsCursorPos cursor = _document.CursorPosition;

            if (cursor < range.Start) {
                _document.Selection = range;
            } else {
                _occurrenceIndex = (_occurrenceIndex + 1) % _occurrences.Length;
                _document.Selection = _occurrences[_occurrenceIndex];
            }
        }

        public void DoFindPrevious() {
            UpdateIfNeeded();
            if (_occurrences.Length == 0) return;

            _occurrenceIndex = (_occurrenceIndex - 1 + _occurrences.Length) % _occurrences.Length;
            _document.Selection = _occurrences[_occurrenceIndex];
        }

        public void DoReplace() {
            UpdateIfNeeded();
            if (_occurrences.Length == 0) return;

            _actionStack.ExecuteAction(ChangeText.Replace(_document, _document.Selection, ReplaceText));
        }

        public void DoReplaceAll() {
            UpdateIfNeeded();
            if (_occurrences.Length == 0) return;

            CompoundAction action = new();
            for (int i = _occurrences.Length - 1; i >= 0; i--) {
                action.AddAction(ChangeText.Replace(_document, _occurrences[i], ReplaceText));
            }

            _actionStack.ExecuteAction(action);
        }

        private void UpdateIfNeeded() {
            if (_occurrencesDirty) {
                UpdateOccurrences();
            } else if (_occurrenceIndexDirty) {
                UpdateOccurrenceIndex();
            }
        }

        private void UpdateOccurrences() {
            if (string.IsNullOrEmpty(_findText)) {
                _occurrences = Array.Empty<StsCursorRange>();
                _occurrenceIndex = -1;
                return;
            }

            string text = _document.GetText(new StsCursorRange(StsCursorPos.Zero, _document.EndPosition));
            if (string.IsNullOrEmpty(text)) {
                _occurrences = Array.Empty<StsCursorRange>();
                _occurrenceIndex = -1;
                return;
            }

            string regex = Regex.Escape(_findText!);
            MatchCollection matches = Regex.Matches(text, regex, RegexOptions.IgnoreCase);

            _occurrences = new StsCursorRange[matches.Count];

            int textIndex = 0;
            int line = 0;
            int col = 0;

            for (int i = 0; i < matches.Count; i++) {
                Match match = matches[i];

                int matchEndIndex = match.Index + match.Length;
                StsCursorPos matchStart = default;
                while (textIndex < matchEndIndex && textIndex < text.Length) {
                    if (textIndex == match.Index) {
                        matchStart = new StsCursorPos(line, col);
                    }

                    char c = text[textIndex++];
                    if (c == '\n') {
                        line++;
                        col = 0;
                    } else {
                        col++;
                    }

                    if (textIndex == matchEndIndex) {
                        StsCursorPos matchEnd = new(line, col);
                        _occurrences[i] = new StsCursorRange(matchStart, matchEnd);
                    }
                }
            }

            _occurrencesDirty = false;

            UpdateOccurrenceIndex();
        }

        private void UpdateOccurrenceIndex() {
            if (_occurrences.Length == 0) {
                _occurrenceIndex = -1;
                _occurrenceIndexDirty = false;
                return;
            }

            StsCursorPos cursorPos = _document.CursorPosition;

            int maxIndex = -1;
            for (int i = 0; i < _occurrences.Length; i++) {
                if (cursorPos <= _occurrences[i].End) {
                    maxIndex = i;
                    break;
                }
            }

            if (maxIndex == -1) {
                maxIndex = 0; // Wrap around
            }

            _occurrenceIndex = maxIndex;
            _occurrenceIndexDirty = false;
        }

        private void HandleLinesChanged(StsRange _) {
            _occurrencesDirty = true;
            _occurrenceIndexDirty = true;
            Updated?.Invoke();
        }

        private void HandleSelectionChanged(StsCursorRange _) {
            _occurrenceIndexDirty = true;
            Updated?.Invoke();
        }
    }
}
