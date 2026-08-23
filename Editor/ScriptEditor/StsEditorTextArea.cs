#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel;
using Infohazard.StillTimeScript.ViewModel.Actions;
using Infohazard.StillTimeScript.ViewModel.Annotations;
using Infohazard.StillTimeScript.ViewModel.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StillTime.Editor.ScriptEditor {
    [UxmlElement]
    public partial class StsEditorTextArea : VisualElement {
        private const string TextAreaCursorClassName = "text-area--hover";
        private const string TextAreaLinkCursorClassName = "text-area--hover-link";
        private const int LineNumberWidth = 50;
        private const int LineHeight = 20;
        private const string Indent = "    ";
        private const float BlinkCooldown = 1;

        private float _charWidth;
        private readonly IMGUIContainer _imguiContainer;
        private readonly Scroller _verticalScroller;
        private readonly List<string?> _formattedLines = new();

        private bool _cursorBlinkState;
        private int _clickCount;
        private float _minBlinkTime;
        private string[] _placeholderLines = Array.Empty<string>();

        [UxmlAttribute] public GUISkin GuiSkin { get; set; } = null!;

        [UxmlAttribute]
        public string[] PlaceholderLines {
            get => _placeholderLines;
            set {
                _placeholderLines = value;
                ViewModel.Rebuild(_placeholderLines);
            }
        }

        public StsDocumentViewModel ViewModel { get; }

        public ActionStack ActionStack { get; }

        private Vector2 ViewOffset => new(0, ViewModel.ScrollValue * LineHeight);

        private IReadOnlyList<string> Lines => ViewModel.ScriptLines;


        public StsEditorTextArea() {
            _imguiContainer = new IMGUIContainer(OnGui) {
                style = {
                    flexGrow = 1,
                },
            };

            _verticalScroller = new Scroller {
                direction = SliderDirection.Vertical,
            };

            Add(_imguiContainer);
            Add(_verticalScroller);

            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Row;

            _imguiContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _imguiContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _imguiContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _imguiContainer.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _imguiContainer.RegisterCallback<ClickEvent>(OnClick);
            _imguiContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
            _imguiContainer.RegisterCallback<WheelEvent>(OnWheel);
            _verticalScroller.valueChanged += OnScrollValueChanged;
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            ViewModel = new StsDocumentViewModel(Enumerable.Empty<string>());
            ActionStack = new ActionStack(ViewModel);

            ViewModel.LinesInserted += OnViewModelLinesInserted;
            ViewModel.LinesModified += OnViewModelLinesModified;
            ViewModel.LinesRemoved += OnViewModelLinesRemoved;
            ViewModel.CursorChanged += OnViewModelCursorChanged;
            ViewModel.SelectionChanged += OnViewModelSelectionChanged;
            ViewModel.ScrollChanged += OnViewModelScrollChanged;
        }

        #region Event Handlers

        private void OnAttachToPanel(AttachToPanelEvent evt) {
            _charWidth = GuiSkin.label.CalcSize(new GUIContent("A")).x;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            UpdateScrollBar();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) {
            EditorApplication.update -= Update;
        }

        private void Update() {
            if (!ViewModel.CursorActive) return;

            float time = Time.realtimeSinceStartup;
            bool blink = time < _minBlinkTime || time % 1 > 0.5f;
            if (blink == _cursorBlinkState) return;

            _cursorBlinkState = blink;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnMouseDown(MouseDownEvent evt) {
            if (evt.button == 0) {
                _clickCount = evt.clickCount;
                StsCursorPos cursorPos = GetCursorPosition(evt.localMousePosition);
                ViewModel.CursorPosition = cursorPos;

                if (evt.clickCount == 1) {
                    ViewModel.Selection = new StsCursorRange(cursorPos, cursorPos);
                } else if (evt.clickCount == 2) {
                    string line = ViewModel.ScriptLines[cursorPos.Line];

                    int wordStart = cursorPos.Column;
                    for (int i = cursorPos.Column - 1; i >= 0; i--) {
                        if (IsWordChar(line[i])) {
                            wordStart = i;
                        } else {
                            break;
                        }
                    }

                    int wordEnd = cursorPos.Column;
                    for (int i = cursorPos.Column; i < line.Length; i++) {
                        if (IsWordChar(line[i])) {
                            wordEnd = i + 1;
                        } else {
                            break;
                        }
                    }

                    ViewModel.Selection = new StsCursorRange(cursorPos.Line, wordStart, wordEnd);
                    ViewModel.CursorPosition = ViewModel.SelectionEnd;
                } else if (evt.clickCount == 3) {
                    string line = ViewModel.ScriptLines[cursorPos.Line];
                    ViewModel.Selection = new StsCursorRange(cursorPos.Line, 0, line.Length);
                }
            }
        }

        private void OnClick(ClickEvent clickEvent) {
            Rect r = _imguiContainer.contentRect;
            Vector2 mousePos = clickEvent.localPosition;
            Rect textCursorRect = new(r);
            textCursorRect.xMin += LineNumberWidth;

            bool isTextCursor = textCursorRect.Contains(mousePos);
            
            if (isTextCursor && clickEvent.clickCount == 1 && (clickEvent.commandKey || clickEvent.ctrlKey)) {
                
                StsCursorPos cursorPos = GetCursorPosition(mousePos);
                DefinitionReferenceAnnotation? annotation = GetReferenceAtPosition(cursorPos);
                if (annotation is { DefinitionToken: { } defToken  }) {
                    ViewModel.CursorPosition = new StsCursorPos(defToken.LineNumber, defToken.Range.Start);
                    ViewModel.Selection =
                        new StsCursorRange(defToken.LineNumber, defToken.Range.Start, defToken.Range.End);
                }
            }
        }

        private static bool IsWordChar(char c) {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private void OnMouseUp(MouseUpEvent evt) {
            if (evt.button == 0 && _clickCount == 1) {
                ViewModel.CursorPosition = GetCursorPosition(evt.localMousePosition);

                if (ViewModel.CursorPosition == ViewModel.SelectionStart) {
                    ViewModel.ClearSelection();
                } else {
                    ViewModel.SelectionEnd = ViewModel.CursorPosition;
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt) {
            UpdateCursor(evt);

            if ((evt.pressedButtons & 1) != 0) {
                ViewModel.CursorPosition = GetCursorPosition(evt.localMousePosition);
                ViewModel.SelectionEnd = ViewModel.CursorPosition;
            }
        }

        private void UpdateCursor(IMouseEvent evt) {
            Rect r = _imguiContainer.contentRect;

            Vector2 cursorPosition = evt.localMousePosition;
            Rect textCursorRect = new(r);
            textCursorRect.xMin += LineNumberWidth;

            bool isTextCursor = textCursorRect.Contains(cursorPosition);
            _imguiContainer.EnableInClassList(TextAreaCursorClassName, isTextCursor);

            bool isLinkCursor = false;
            if (isTextCursor && (evt.commandKey || evt.ctrlKey)) {
                StsCursorPos cursorPos = GetCursorPosition(evt.localMousePosition);
                DefinitionReferenceAnnotation? reference = GetReferenceAtPosition(cursorPos);
                if (reference != null) {
                    isLinkCursor = true;
                }
            }
            
            _imguiContainer.EnableInClassList(TextAreaLinkCursorClassName, isLinkCursor);
        }

        private void OnWheel(WheelEvent evt) {
            _verticalScroller.value += evt.delta.y;
        }


        private void OnScrollValueChanged(float value) {
            ViewModel.ScrollValue = value;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.keyCode == KeyCode.Backspace) {
                if (ViewModel.SelectionActive) {
                    DeleteSelection();
                } else {
                    DeleteCharBeforeCursor();
                }
            } else if (evt.keyCode == KeyCode.Delete) {
                if (ViewModel.SelectionActive) {
                    DeleteSelection();
                } else {
                    DeleteCharAfterCursor();
                }
            } else if (evt.keyCode == KeyCode.Return) {
                InsertText("\n");
            } else if (evt.keyCode == KeyCode.Tab) {
                if (!evt.shiftKey) {
                    if (ViewModel.SelectionActive) {
                        InsertTextBeforeSelectedLines(Indent);
                    } else {
                        InsertText(Indent);
                    }
                } else {
                    if (ViewModel.SelectionActive) {
                        RemoveTextBeforeSelectedLines(Indent);
                    } else {
                        RemoveTextBeforeCursor(Indent);
                    }
                }
            } else if (evt.keyCode == KeyCode.LeftArrow) {
                if (ViewModel.CursorColumn > 0) {
                    ViewModel.CursorColumn--;
                } else if (ViewModel.CursorLine > 0) {
                    ViewModel.CursorPosition =
                        new StsCursorPos(ViewModel.CursorLine - 1, Lines[ViewModel.CursorLine - 1].Length);
                }
            } else if (evt.keyCode == KeyCode.RightArrow) {
                if (ViewModel.CursorColumn < Lines[ViewModel.CursorLine].Length) {
                    ViewModel.CursorColumn++;
                } else if (ViewModel.CursorLine < Lines.Count - 1) {
                    ViewModel.CursorPosition = new StsCursorPos(ViewModel.CursorLine + 1, 0);
                }
            } else if (evt.keyCode == KeyCode.UpArrow) {
                if (ViewModel.CursorLine > 0) {
                    ViewModel.CursorLine--;
                } else {
                    ViewModel.CursorPosition = StsCursorPos.Zero;
                }
            } else if (evt.keyCode == KeyCode.DownArrow) {
                if (ViewModel.CursorLine < Lines.Count - 1) {
                    ViewModel.CursorLine++;
                } else {
                    ViewModel.CursorPosition = ViewModel.EndPosition;
                }
            } else if (evt.keyCode == KeyCode.End) {
                if (!evt.shiftKey) {
                    ViewModel.CursorColumn = Lines[ViewModel.CursorLine].Length;
                } else {
                    ViewModel.CursorPosition = ViewModel.EndPosition;
                }
            } else if (evt.keyCode == KeyCode.Home) {
                if (!evt.shiftKey) {
                    ViewModel.CursorColumn = 0;
                } else {
                    ViewModel.CursorPosition = StsCursorPos.Zero;
                }
            } else if (evt.ctrlKey || evt.commandKey) {
                if (evt.keyCode == KeyCode.A) {
                    ViewModel.Selection = new StsCursorRange(StsCursorPos.Zero, ViewModel.EndPosition);
                } else if (evt.keyCode == KeyCode.C) {
                    string text = ViewModel.GetSelectedText() ?? string.Empty;
                    GUIUtility.systemCopyBuffer = text;
                } else if (evt.keyCode == KeyCode.X) {
                    string text = ViewModel.GetSelectedText() ?? string.Empty;
                    GUIUtility.systemCopyBuffer = text;
                    DeleteSelection();
                } else if (evt.keyCode == KeyCode.V) {
                    string text = GUIUtility.systemCopyBuffer;
                    InsertText(text);
                } else if (evt is { keyCode: KeyCode.Z, shiftKey: false }) {
                    ActionStack.Undo();
                } else if (evt.keyCode == KeyCode.Y || evt is { keyCode: KeyCode.Z, shiftKey: true }) {
                    ActionStack.Redo();
                }
            } else if (evt.character is not ('\t' or '\0' or '\n')) {
                InsertText(evt.character.ToString());
            }

            _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            UpdateScrollBar();
        }

        #endregion

        #region Link Handling

        private DefinitionReferenceAnnotation? GetReferenceAtPosition(StsCursorPos position) {
            List<LineAnnotation>? annotations = ViewModel.GetAnnotations(position.Line);
            if (annotations == null) return null;

            foreach (DefinitionReferenceAnnotation annotation in annotations.OfType<DefinitionReferenceAnnotation>()) {
                StsRange range = annotation.Range;
                if (position.Column >= range.Start && position.Column <= range.End) {
                    return annotation;
                }
            }

            return null;
        }

        #endregion

        #region ViewModel Events

        private void OnViewModelLinesInserted(StsRange range) {
            _formattedLines.InsertRange(range.Start, Enumerable.Repeat<string?>(null, range.Length));
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelLinesRemoved(StsRange range) {
            _formattedLines.RemoveRange(range.Start, range.Length);
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelLinesModified(StsRange range) {
            for (int i = range.Start; i < range.End; i++) {
                _formattedLines[i] = null;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelCursorChanged(StsCursorPos cursor) {
            _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
            _cursorBlinkState = true;
            
            int visibleLineCount = Mathf.FloorToInt(_imguiContainer.contentRect.height / LineHeight);
            int maxVisibleLine = Mathf.FloorToInt(ViewModel.ScrollValue + visibleLineCount);

            if (ViewModel.CursorLine < ViewModel.ScrollValue) {
                _verticalScroller.value = ViewModel.CursorLine;
            } else if (ViewModel.CursorLine > maxVisibleLine) {
                _verticalScroller.value = ViewModel.CursorLine - visibleLineCount + 1;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelSelectionChanged(StsCursorRange range) {
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelScrollChanged(float value) {
            _verticalScroller.value = value;
            _imguiContainer.MarkDirtyRepaint();
        }

        #endregion

        #region Modification Commands

        private void DeleteSelection() {
            ActionStack.ExecuteAction(ChangeText.Delete(ViewModel, ViewModel.Selection));
        }

        private void DeleteCharBeforeCursor() {
            StsCursorPos deleteMin = ViewModel.CursorPosition;
            StsCursorPos deleteMax = ViewModel.CursorPosition;

            int minCol = deleteMin.Column;
            int minLine = deleteMin.Line;

            if (minCol > 0) {
                minCol--;
            } else if (minLine > 0) {
                minLine--;
                minCol = Lines[minLine].Length;
            }

            ActionStack.ExecuteAction(
                ChangeText.Delete(ViewModel, new StsCursorRange(minLine, minCol, deleteMax.Line, deleteMax.Column)));
        }

        private void DeleteCharAfterCursor() {
            StsCursorPos deleteMin = ViewModel.CursorPosition;
            StsCursorPos deleteMax = ViewModel.CursorPosition;

            int maxCol = deleteMax.Column;
            int maxLine = deleteMax.Line;

            if (maxCol < Lines[maxLine].Length) {
                maxCol++;
            } else if (maxLine < Lines.Count) {
                maxLine++;
                maxCol = 0;
            }

            ActionStack.ExecuteAction(
                ChangeText.Delete(ViewModel, new StsCursorRange(deleteMin.Line, deleteMin.Column, maxLine, maxCol)));
        }

        private void InsertText(string text) {
            if (ViewModel.SelectionActive) {
                ActionStack.ExecuteAction(ChangeText.Replace(ViewModel, ViewModel.Selection, text));   
            } else {
                ActionStack.ExecuteAction(ChangeText.Insert(ViewModel.CursorPosition, text));
            }
        }

        private void InsertTextBeforeSelectedLines(string text) {
            StsCursorRange range = ViewModel.Selection;
            int minLine = range.Min.Line;
            int maxLine = range.Max.Line;

            CompoundAction action = new() {
                OriginalCursorPosition = ViewModel.CursorPosition,
                OriginalSelection = ViewModel.Selection,
                NewCursorPosition = new StsCursorPos(ViewModel.CursorLine, ViewModel.CursorColumn + text.Length),
                NewSelection = new StsCursorRange(
                    range.Start.Line, range.Start.Column + text.Length,
                    range.End.Line, range.End.Column + text.Length),
            };

            for (int i = minLine; i <= maxLine && i < Lines.Count; i++) {
                action.AddAction(ChangeText.Insert(new StsCursorPos(i, 0), text));
            }

            ActionStack.ExecuteAction(action);
        }

        private void RemoveTextBeforeSelectedLines(string text) {
            StsCursorRange range = ViewModel.Selection;
            int minLine = range.Min.Line;
            int maxLine = range.Max.Line;

            IReadOnlyList<string> lines = Lines;

            StsCursorPos start = ViewModel.SelectionStart;
            StsCursorPos end = ViewModel.SelectionEnd;

            CompoundAction action = new() {
                OriginalCursorPosition = ViewModel.CursorPosition,
                OriginalSelection = ViewModel.Selection,
            };

            for (int i = minLine; i <= maxLine && i < lines.Count; i++) {
                int j;
                string line = lines[i];
                for (j = 0; j < text.Length && j < line.Length; j++) {
                    if (line[j] != text[j]) break;
                }

                action.AddAction(
                    ChangeText.Delete(ViewModel, new StsCursorRange(new StsCursorPos(i, 0), new StsCursorPos(i, j))));

                if (i == start.Line) {
                    start = new StsCursorPos(start.Line, start.Column - j);
                }

                if (i == end.Line) {
                    end = new StsCursorPos(end.Line, end.Column - j);
                }

                if (i == ViewModel.CursorLine) {
                    action.NewCursorPosition = new StsCursorPos(ViewModel.CursorLine, ViewModel.CursorColumn - j);
                }
            }

            action.NewSelection = new StsCursorRange(start, end);

            ActionStack.ExecuteAction(action);
        }

        private void RemoveTextBeforeCursor(string text) {
            IReadOnlyList<string> lines = Lines;
            string line = lines[ViewModel.CursorLine];

            int i;
            for (i = 0; i < text.Length && i < ViewModel.CursorColumn; i++) {
                int x = ViewModel.CursorColumn - (i + 1);
                if (line[x] != text[^(i + 1)]) break;
            }

            ActionStack.ExecuteAction(
                ChangeText.Delete(
                    ViewModel,
                    new StsCursorRange(
                        new StsCursorPos(ViewModel.CursorLine, ViewModel.CursorColumn - i),
                        ViewModel.CursorPosition)));
        }

        #endregion

        #region Scroll Handling

        private StsCursorPos GetCursorPosition(Vector2 localMousePosition) {
            IReadOnlyList<string> lines = Lines;
            Rect r = _imguiContainer.contentRect;

            Vector2 docMousePos = ViewOffset + localMousePosition - r.min - new Vector2(LineNumberWidth, 0);

            int lineNumber = Mathf.Clamp(Mathf.FloorToInt(docMousePos.y / LineHeight), 0, lines.Count - 1);
            int lineLength = lines[lineNumber].Length;
            int charIndex = Mathf.Clamp(Mathf.RoundToInt(docMousePos.x / _charWidth), 0, lineLength);

            return new StsCursorPos(lineNumber, charIndex);
        }

        private void UpdateScrollBar() {
            float height = _imguiContainer.contentRect.height;
            int extraLines = Mathf.RoundToInt(height / LineHeight);
            _verticalScroller.lowValue = 0;
            _verticalScroller.highValue = Mathf.Max(0, Lines.Count);

            float ratio = _imguiContainer.contentRect.height / ((Lines.Count + extraLines) * LineHeight);
            _verticalScroller.Adjust(ratio);
        }

        #endregion

        #region Line Formatting

        private string GetFormattedLine(int index) {
            if (index < 0 || index >= Lines.Count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            string? formattedLine = _formattedLines[index];
            if (formattedLine == null) {
                formattedLine = ScriptLineFormatter.FormatLine(ViewModel, index);
                _formattedLines[index] = formattedLine;
            }

            return formattedLine;
        }

        #endregion

        #region Drawing

        private void OnGui() {
            IReadOnlyList<string> lines = Lines;
            GUISkin oldSkin = GUI.skin;
            GUI.skin = GuiSkin;

            try {
                Rect r = _imguiContainer.contentRect;

                float scrollValue = ViewModel.ScrollValue;
                int minVisibleLine =
                    Mathf.Clamp(Mathf.FloorToInt(scrollValue), 0, lines.Count);
                int maxVisibleLine =
                    Mathf.Clamp(Mathf.CeilToInt(scrollValue + r.height / LineHeight), 0, lines.Count);

                for (int i = minVisibleLine; i < maxVisibleLine; i++) {
                    float y = i - scrollValue;
                    Rect rect = new(r.x, r.y + y * LineHeight, r.width, LineHeight);
                    DrawLineBg(i, rect);

                    Rect lineNumberRect = new(rect.x, rect.y, LineNumberWidth, LineHeight);
                    DrawLineNumber(lineNumberRect, i);

                    Rect lineRect = rect;
                    lineRect.xMin = lineNumberRect.xMax;

                    if (ViewModel.SelectionActive) {
                        DrawSelectionRect(i, lineRect, lines[i].Length);
                    }

                    DrawTextLine(lineRect, GetFormattedLine(i));
                }

                if (ViewModel.CursorActive) {
                    DrawCursor();
                }

                GUI.color = new Color(0.1372549f, 0.1372549f, 0.1372549f, 1);
                GUI.DrawTexture(new Rect(r.xMin + LineNumberWidth - 3, r.yMin, 2, r.height),
                                EditorGUIUtility.whiteTexture);
                GUI.color = Color.white;
            } finally {
                GUI.skin = oldSkin;
            }
        }

        private void DrawLineBg(int i, Rect rect) {
            if (i % 2 != 1) return;

            GUI.color = new Color(0, 0, 0, 0.1f);
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawSelectionRect(int i, Rect rect, int lineLength) {
            StsCursorRange range = ViewModel.Selection;
            StsCursorPos selectionMin = range.Min;
            StsCursorPos selectionMax = range.Max;

            if (i < selectionMin.Line || i > selectionMax.Line) return;

            RangeInt selectionRange = new(0, lineLength);

            if (i == selectionMin.Line) {
                selectionRange.start = selectionMin.Column;
                selectionRange.length = lineLength - selectionRange.start;
            }

            if (i == selectionMax.Line) {
                selectionRange.length = selectionMax.Column - selectionRange.start;
            }

            if (i < selectionMax.Line) {
                selectionRange.length += 1;
            }

            Rect selectionRect = rect;
            selectionRect.xMin += selectionRange.start * _charWidth;
            selectionRect.width = selectionRange.length * _charWidth;

            GUI.color = new Color(0.2f, 0.2f, 0.5f, 1);
            GUI.DrawTexture(selectionRect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawCursor() {
            if (!_cursorBlinkState) return;
            Rect rect = _imguiContainer.contentRect;
            Vector2 offset = rect.min - ViewOffset;
            float cursorX = LineNumberWidth + offset.x + ViewModel.CursorColumn * _charWidth;
            float cursorY = offset.y + ViewModel.CursorLine * LineHeight;

            Rect cursorRect = new(cursorX - 1, cursorY + 2, 2, LineHeight - 2);

            GUI.color = Color.white;
            GUI.DrawTexture(cursorRect, EditorGUIUtility.whiteTexture);
        }

        private static void DrawLineNumber(Rect rect, int i) {
            Rect paddedRect = rect;
            paddedRect.xMin += 5;
            paddedRect.xMax -= 5;

            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.Label(paddedRect, $"{i + 1}");
            GUI.color = Color.white;
        }

        private static void DrawTextLine(Rect rect, string line) {
            GUI.color = Color.white;
            GUI.Label(rect, line);
        }

        #endregion
    }
}
