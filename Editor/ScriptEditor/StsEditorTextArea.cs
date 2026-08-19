#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.ViewModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StillTime.Editor.ScriptEditor {
    [UxmlElement]
    public partial class StsEditorTextArea : VisualElement {
        private const string TextAreaCursorClassName = "HoverText";
        private const int LineNumberWidth = 50;
        private const int LineHeight = 20;
        private const string Indent = "    ";
        private const float BlinkCooldown = 1;

        private float _charWidth;
        private readonly IMGUIContainer _imguiContainer;
        private readonly Scroller _verticalScroller;
        private readonly List<string?> _formattedLines = new();

        private bool _cursorBlinkState;
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
            _imguiContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
            _imguiContainer.RegisterCallback<WheelEvent>(OnWheel);
            _verticalScroller.valueChanged += OnScrollValueChanged;
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            ViewModel = new StsDocumentViewModel(Enumerable.Empty<string>());
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
                ViewModel.CursorPosition = GetCursorPosition(evt.localMousePosition);
                ViewModel.SetSelectionRange(ViewModel.CursorPosition, ViewModel.CursorPosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt) {
            if (evt.button == 0) {
                ViewModel.CursorPosition = GetCursorPosition(evt.localMousePosition);

                if (ViewModel.CursorPosition == ViewModel.SelectionStart) {
                    ViewModel.ClearSelection();
                } else {
                    ViewModel.SelectionEnd = ViewModel.CursorPosition;
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt) {
            Rect r = _imguiContainer.contentRect;

            Vector2 cursorPosition = evt.localMousePosition;
            Rect textCursorRect = new(r);
            textCursorRect.xMin += LineNumberWidth;

            bool isTextCursor = textCursorRect.Contains(cursorPosition);
            _imguiContainer.EnableInClassList(TextAreaCursorClassName, isTextCursor);

            if ((evt.pressedButtons & 1) != 0) {
                ViewModel.CursorPosition = GetCursorPosition(evt.localMousePosition);
                ViewModel.SelectionEnd = ViewModel.CursorPosition;
            }
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
                if (ViewModel.SelectionActive) {
                    DeleteSelection();
                }

                InsertNewLine();
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
                if (ViewModel.CursorX > 0) {
                    ViewModel.CursorX--;
                } else if (ViewModel.CursorY > 0) {
                    ViewModel.CursorPosition = new Vector2Int(Lines[ViewModel.CursorY - 1].Length, ViewModel.CursorY - 1);
                }
            } else if (evt.keyCode == KeyCode.RightArrow) {
                if (ViewModel.CursorX < Lines[ViewModel.CursorY].Length) {
                    ViewModel.CursorX++;
                } else if (ViewModel.CursorY < Lines.Count - 1) {
                    ViewModel.CursorPosition = new Vector2Int(0, ViewModel.CursorY + 1);
                }
            } else if (evt.keyCode == KeyCode.UpArrow) {
                if (ViewModel.CursorY > 0) {
                    ViewModel.CursorY--;
                } else {
                    ViewModel.CursorPosition = new Vector2Int(0, 0);
                }
            } else if (evt.keyCode == KeyCode.DownArrow) {
                if (ViewModel.CursorY < Lines.Count - 1) {
                    ViewModel.CursorY++;
                } else {
                    ViewModel.CursorPosition = new Vector2Int(Lines[^1].Length, Lines.Count - 1);
                }
            } else if (evt.keyCode == KeyCode.End) {
                if (!evt.shiftKey) {
                    ViewModel.CursorX = Lines[ViewModel.CursorY].Length;
                } else {
                    ViewModel.CursorPosition = new Vector2Int(Lines[^1].Length, Lines.Count - 1);
                }
            } else if (evt.keyCode == KeyCode.Home) {
                if (!evt.shiftKey) {
                    ViewModel.CursorX = 0;
                } else {
                    ViewModel.CursorPosition = new Vector2Int(0, 0);
                }
            } else if (evt.ctrlKey || evt.commandKey) {
                if (evt.keyCode == KeyCode.A) {
                    ViewModel.SetSelectionRange(Vector2Int.zero, ViewModel.EndPosition);
                }
            } else if (evt.character is not ('\t' or '\0' or '\n')) {
                InsertText(evt.character.ToString());
            }

            _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
            int visibleLineCount = Mathf.FloorToInt(_imguiContainer.contentRect.height / LineHeight);
            int maxVisibleLine = Mathf.FloorToInt(ViewModel.ScrollValue + visibleLineCount);

            if (ViewModel.CursorY < ViewModel.ScrollValue) {
                _verticalScroller.value = ViewModel.CursorY;
            } else if (ViewModel.CursorY > maxVisibleLine) {
                _verticalScroller.value = ViewModel.CursorY - visibleLineCount + 1;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            UpdateScrollBar();
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

        private void OnViewModelCursorChanged(Vector2Int cursor) {
            _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
            _cursorBlinkState = true;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelSelectionChanged(Vector2Int start, Vector2Int end) {
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnViewModelScrollChanged(float value) {
            _verticalScroller.value = value;
            _imguiContainer.MarkDirtyRepaint();
        }

        #endregion

        #region Modification Commands

        private void DeleteSelection() {
            Vector2Int min = ViewModel.SelectionMin;
            Vector2Int max = ViewModel.SelectionMax;

            ViewModel.DeleteText(min, max);
            ViewModel.CursorPosition = min;
            ViewModel.ClearSelection();
            _imguiContainer.MarkDirtyRepaint();
        }

        private void DeleteCharBeforeCursor() {
            Vector2Int deleteMin = ViewModel.CursorPosition;
            Vector2Int deleteMax = ViewModel.CursorPosition;

            if (deleteMin.x > 0) {
                deleteMin.x--;
            } else if (deleteMin.y > 0) {
                deleteMin = new Vector2Int(Lines[deleteMin.y - 1].Length, deleteMin.y - 1);
            }

            ViewModel.DeleteText(deleteMin, deleteMax);
            ViewModel.CursorPosition = deleteMin;
            ViewModel.ClearSelection();
            _imguiContainer.MarkDirtyRepaint();
        }

        private void DeleteCharAfterCursor() {
            Vector2Int deleteMin = ViewModel.CursorPosition;
            Vector2Int deleteMax = ViewModel.CursorPosition;

            if (deleteMin.x < Lines[deleteMin.y].Length) {
                deleteMax.x++;
            } else if (deleteMin.y < Lines.Count) {
                deleteMax = new Vector2Int(0, deleteMin.y + 1);
            }

            ViewModel.DeleteText(deleteMin, deleteMax);
            ViewModel.CursorPosition = deleteMin;
            ViewModel.ClearSelection();
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertNewLine() {
            ViewModel.InsertNewLine(ViewModel.CursorPosition);
            ViewModel.CursorPosition = new Vector2Int(0, ViewModel.CursorY + 1);
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertText(string text) {
            ViewModel.InsertText(ViewModel.CursorPosition, text);
            ViewModel.CursorX += text.Length;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertTextBeforeSelectedLines(string text) {
            int min = ViewModel.SelectionMin.y;
            int max = ViewModel.SelectionMax.y;

            for (int i = min; i <= max && i < Lines.Count; i++) {
                ViewModel.InsertText(new Vector2Int(0, i), text);
            }

            ViewModel.CursorX += text.Length;
            Vector2Int offset = new(1, 0);
            ViewModel.SetSelectionRange(ViewModel.SelectionStart + offset, ViewModel.SelectionEnd + offset);
            _imguiContainer.MarkDirtyRepaint();
        }

        private void RemoveTextBeforeSelectedLines(string text) {
            int min = ViewModel.SelectionMin.y;
            int max = ViewModel.SelectionMax.y;

            IReadOnlyList<string> lines = Lines;

            for (int i = min; i <= max && i < lines.Count; i++) {
                int j;
                string line = lines[i];
                for (j = 0; j < text.Length && j < line.Length; j++) {
                    if (line[j] != text[j]) break;
                }

                ViewModel.DeleteText(new Vector2Int(0, i), new Vector2Int(j, i));

                Vector2Int offset = new(-1, 0);
                if (i == ViewModel.SelectionStart.y) ViewModel.SelectionStart += offset;
                if (i == ViewModel.SelectionEnd.y) ViewModel.SelectionEnd += offset;
                if (i == ViewModel.CursorY) ViewModel.CursorX -= j;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void RemoveTextBeforeCursor(string text) {
            IReadOnlyList<string> lines = Lines;
            string line = lines[ViewModel.CursorY];

            int i;
            for (i = 0; i < text.Length && i < ViewModel.CursorX; i++) {
                int x = ViewModel.CursorX - (i + 1);
                if (line[x] != text[^(i + 1)]) break;
            }

            ViewModel.DeleteText(new Vector2Int(ViewModel.CursorX - i, ViewModel.CursorY), ViewModel.CursorPosition);
            ViewModel.CursorX -= i;
        }

        #endregion

        #region Scroll Handling

        private Vector2Int GetCursorPosition(Vector2 localMousePosition) {
            IReadOnlyList<string> lines = Lines;
            Rect r = _imguiContainer.contentRect;

            Vector2 docMousePos = ViewOffset + localMousePosition - r.min - new Vector2(LineNumberWidth, 0);

            int lineNumber = Mathf.Clamp(Mathf.FloorToInt(docMousePos.y / LineHeight), 0, lines.Count - 1);
            int lineLength = lines[lineNumber].Length;
            int charIndex = Mathf.Clamp(Mathf.RoundToInt(docMousePos.x / _charWidth), 0, lineLength);

            return new Vector2Int(charIndex, lineNumber);
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
            Vector2Int selectionMin = ViewModel.SelectionMin;
            Vector2Int selectionMax = ViewModel.SelectionMax;

            if (i < selectionMin.y || i > selectionMax.y) return;

            RangeInt selectionRange = new(0, lineLength);

            if (i == selectionMin.y) {
                selectionRange.start = selectionMin.x;
                selectionRange.length = lineLength - selectionRange.start;
            }

            if (i == selectionMax.y) {
                selectionRange.length = selectionMax.x - selectionRange.start;
            }

            if (i < selectionMax.y) {
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
            float cursorX = LineNumberWidth + offset.x + ViewModel.CursorX * _charWidth;
            float cursorY = offset.y + ViewModel.CursorY * LineHeight;

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
