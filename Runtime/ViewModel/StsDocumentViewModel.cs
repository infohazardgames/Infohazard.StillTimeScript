using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.ViewModel.Annotations;
using UnityEditor;
using UnityEngine;

namespace Infohazard.StillTimeScript.ViewModel {
    public class StsDocumentViewModel : IDisposable {
        private readonly List<string> _scriptLines;
        private readonly List<string> _displayLines;
        private readonly StringBuilder _stringBuilder;
        private List<List<LineAnnotation>> _annotations;
        private bool _needsToUpdateAnnotations;

        private CancellationTokenSource _annotationCancellationTokenSource;
        private Task _annotationTask;
        private List<List<LineAnnotation>> _pendingAnnotations;

        public IReadOnlyList<string> ScriptLines => _scriptLines;

        public IReadOnlyList<string> DisplayLines => _displayLines;

        public IReadOnlyList<IReadOnlyList<LineAnnotation>> Annotations => _annotations;

        public bool IsModified { get; private set; }

        public event Action<bool> IsModifiedChanged;
        public event Action AnnotationsChanged;

        public StsDocumentViewModel(IEnumerable<string> scriptLines) {
            _scriptLines = new List<string>(scriptLines);
            _displayLines = new List<string>();
            _stringBuilder = new StringBuilder();
            for (int i = 0; i < _scriptLines.Count; i++) {
                _displayLines.Add(ConvertLineForDisplay(i));
            }

            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            UpdateAnnotations();
        }

        public void Dispose() {
            EditorApplication.update -= Update;
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
                _displayLines.RemoveRange(minPosition.y + 1, maxPosition.y - minPosition.y);
                _annotations?.RemoveRange(minPosition.y + 1, maxPosition.y - minPosition.y);
            }

            _scriptLines[minPosition.y] = keptContentOnFirstLine + keptContentOnLastLine;
            _displayLines[minPosition.y] = ConvertLineForDisplay(minPosition.y);

            if (_annotations != null) {
                _annotations[minPosition.y] = new List<LineAnnotation>();
                AnnotationsChanged?.Invoke();
            }

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
            _displayLines[position.y] = ConvertLineForDisplay(position.y);

            _scriptLines.Insert(position.y + 1, nextLine);
            _displayLines.Insert(position.y + 1, ConvertLineForDisplay(position.y + 1));

            if (_annotations != null) {
                _annotations.Insert(position.y + 1, new List<LineAnnotation>());
                AnnotationsChanged?.Invoke();
            }

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
            _displayLines[position.y] = ConvertLineForDisplay(position.y);
            if (_annotations != null) {
                _annotations[position.y] = new List<LineAnnotation>();
                AnnotationsChanged?.Invoke();
            }

            IsModified = true;
            IsModifiedChanged?.Invoke(IsModified);

            UpdateAnnotations();
        }

        public void ClearModified() {
            IsModified = false;
            IsModifiedChanged?.Invoke(IsModified);
        }

        private string ConvertLineForDisplay(int index) {
            List<LineAnnotation> annotations = _annotations?[index];
            string line = _scriptLines[index];
            _stringBuilder.Clear();

            if (annotations?.Count > 0) {
                annotations.Sort((a, b) => a.RangeStart.CompareTo(b.RangeStart));
                int curIndex = 0;

                foreach (LineAnnotation annotation in annotations) {
                    if (annotation.RangeStart < curIndex) continue;

                    if (annotation.RangeStart > curIndex) {
                        AppendNoParse(line.AsSpan(curIndex, annotation.RangeStart - curIndex));
                    }

                    _stringBuilder.Append(annotation.StartText);
                    AppendNoParse(line.AsSpan(annotation.RangeStart, annotation.RangeEnd - annotation.RangeStart));
                    _stringBuilder.Append(annotation.EndText);
                    curIndex = annotation.RangeEnd;
                }

                if (curIndex < line.Length) {
                    AppendNoParse(line.AsSpan(curIndex));
                }
            } else {
                AppendNoParse(line);
            }

            return _stringBuilder.ToString();
        }

        private void AppendNoParse(ReadOnlySpan<char> text) {
            _stringBuilder.Append("<noparse>");
            _stringBuilder.Append(text);
            _stringBuilder.Append("</noparse>");
        }

        private void UpdateAnnotations() {
            _needsToUpdateAnnotations = true;
        }

        private void CancelAnnotation() {
            _annotationCancellationTokenSource?.Cancel();

            try {
                _annotationTask.Wait();
            } catch {
                // Ignored
            }

            _annotationCancellationTokenSource?.Dispose();
            _annotationCancellationTokenSource = null;
        }

        private void Update() {
            if (_needsToUpdateAnnotations) {
                _needsToUpdateAnnotations = false;
                CancelAnnotation();
                _annotationCancellationTokenSource = new CancellationTokenSource();

                List<string> copiedLines = new(_scriptLines);
                CancellationToken cancellationToken = _annotationCancellationTokenSource.Token;

                _annotationTask = Task.Run(() => {
                    List<List<LineAnnotation>> annotations = Annotator.Annotate(copiedLines);
                    cancellationToken.ThrowIfCancellationRequested();
                    _pendingAnnotations = annotations;
                }, cancellationToken);

                _annotationTask.AsUniTask().Forget();
            }

            if (_pendingAnnotations != null) {
                _annotations = _pendingAnnotations;
                _pendingAnnotations = null;
                _displayLines.Clear();
                for (int i = 0; i < _scriptLines.Count; i++) {
                    _displayLines.Add(ConvertLineForDisplay(i));
                }

                AnnotationsChanged?.Invoke();
            }
        }
    }
}
