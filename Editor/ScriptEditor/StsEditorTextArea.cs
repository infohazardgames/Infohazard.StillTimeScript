using System.Collections.Generic;
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

        [UxmlAttribute] public GUISkin GuiSkin { get; set; }

        [UxmlAttribute] public string[] PlaceholderLines { get; set; }

        public StsDocumentViewModel ViewModel {
            get => _viewModel;
            set {
                if (_viewModel == value) return;

                if (_viewModel != null) {
                    _viewModel.AnnotationsChanged -= OnAnnotationsChanged;
                }

                _viewModel = value;

                if (_viewModel != null) {
                    _viewModel.AnnotationsChanged += OnAnnotationsChanged;
                }
            }
        }

        private float _currentScrollValue;
        private bool _selectionActive;
        private bool _cursorActive;
        private bool _cursorBlinkState;
        private Vector2Int _cursorPosition;
        private Vector2Int _selectionStart;
        private Vector2Int _selectionEnd;
        private float _minBlinkTime;
        private int _rememberedCursorX;
        private StsDocumentViewModel _viewModel;

        private IReadOnlyList<string> Lines => ViewModel?.ScriptLines ?? PlaceholderLines;
        private IReadOnlyList<string> DisplayLines => ViewModel?.DisplayLines ?? PlaceholderLines;

        private bool SelectionReverse => _selectionStart.y == _selectionEnd.y
            ? _selectionStart.x > _selectionEnd.x
            : _selectionStart.y > _selectionEnd.y;

        private Vector2Int SelectionMin => SelectionReverse ? _selectionEnd : _selectionStart;
        private Vector2Int SelectionMax => SelectionReverse ? _selectionStart : _selectionEnd;

        private Vector2 ViewOffset => new(0, _currentScrollValue * LineHeight);

        public Vector2Int CursorPosition {
            get => _cursorPosition;
            private set {
                if (_cursorPosition == value) return;
                _rememberedCursorX = value.x;
                _cursorPosition.y = Mathf.Clamp(value.y, 0, Lines.Count - 1);
                _cursorPosition.x = Mathf.Clamp(value.x, 0, Lines[_cursorPosition.y].Length);
                _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
                _cursorBlinkState = true;
                _cursorActive = true;
                _imguiContainer.MarkDirtyRepaint();
            }
        }

        public int CursorX {
            get => _cursorPosition.x;
            set => CursorPosition = new Vector2Int(value, _cursorPosition.y);
        }

        public int CursorY {
            get => _cursorPosition.y;
            set => CursorPosition = new Vector2Int(_rememberedCursorX, value);
        }

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
            if (!_cursorActive) return;

            float time = Time.realtimeSinceStartup;
            bool blink = time < _minBlinkTime || time % 1 > 0.5f;
            if (blink == _cursorBlinkState) return;

            _cursorBlinkState = blink;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnAnnotationsChanged() {
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnMouseDown(MouseDownEvent evt) {
            if (evt.button == 0) {
                CursorPosition = GetCursorPosition(evt.localMousePosition);
                _selectionStart = CursorPosition;
                _selectionEnd = CursorPosition;
                _selectionActive = true;
                _cursorActive = true;
            }
        }

        private void OnMouseUp(MouseUpEvent evt) {
            if (evt.button == 0) {
                CursorPosition = GetCursorPosition(evt.localMousePosition);

                _selectionActive = CursorPosition != _selectionStart;
                if (_selectionActive) {
                    _selectionEnd = CursorPosition;
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
                CursorPosition = GetCursorPosition(evt.localMousePosition);
                if (_selectionActive) {
                    _selectionEnd = CursorPosition;
                }
            }
        }

        private void OnWheel(WheelEvent evt) {
            _verticalScroller.value += evt.delta.y;
        }


        private void OnScrollValueChanged(float value) {
            _currentScrollValue = value;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.keyCode == KeyCode.Backspace) {
                if (_selectionActive) {
                    DeleteSelection();
                } else {
                    DeleteCharBeforeCursor();
                }
            } else if (evt.keyCode == KeyCode.Delete) {
                if (_selectionActive) {
                    DeleteSelection();
                } else {
                    DeleteCharAfterCursor();
                }
            } else if (evt.keyCode == KeyCode.Return) {
                if (_selectionActive) {
                    DeleteSelection();
                }

                InsertNewLine();
            } else if (evt.keyCode == KeyCode.Tab) {
                if (!evt.shiftKey) {
                    if (_selectionActive) {
                        InsertTextBeforeSelectedLines(Indent);
                    } else {
                        InsertText(Indent);
                    }
                } else {
                    if (_selectionActive) {
                        RemoveTextBeforeSelectedLines(Indent);
                    } else {
                        RemoveTextBeforeCursor(Indent);
                    }
                }
            } else if (evt.keyCode == KeyCode.LeftArrow) {
                if (CursorX > 0) {
                    CursorX--;
                } else if (CursorY > 0) {
                    CursorPosition = new Vector2Int(Lines[CursorY - 1].Length, CursorY - 1);
                }
            } else if (evt.keyCode == KeyCode.RightArrow) {
                if (CursorX < Lines[CursorY].Length) {
                    CursorX++;
                } else if (CursorY < Lines.Count - 1) {
                    CursorPosition = new Vector2Int(0, CursorY + 1);
                }
            } else if (evt.keyCode == KeyCode.UpArrow) {
                if (CursorY > 0) {
                    CursorY--;
                } else {
                    CursorPosition = new Vector2Int(0, 0);
                }
            } else if (evt.keyCode == KeyCode.DownArrow) {
                if (CursorY < Lines.Count - 1) {
                    CursorY++;
                } else {
                    CursorPosition = new Vector2Int(Lines[^1].Length, Lines.Count - 1);
                }
            } else if (evt.keyCode == KeyCode.End) {
                if (!evt.shiftKey) {
                    CursorX = Lines[CursorY].Length;
                } else {
                    CursorPosition = new Vector2Int(Lines[^1].Length, Lines.Count - 1);
                }
            } else if (evt.keyCode == KeyCode.Home) {
                if (!evt.shiftKey) {
                    CursorX = 0;
                } else {
                    CursorPosition = new Vector2Int(0, 0);
                }
            } else if (evt.ctrlKey || evt.commandKey) {
                if (evt.keyCode == KeyCode.A) {
                    _selectionStart = new Vector2Int(0, 0);
                    _selectionEnd = new Vector2Int(Lines[^1].Length, Lines.Count - 1);
                }
            } else if (evt.character is not ('\t' or '\0' or '\n')) {
                InsertText(evt.character.ToString());
            }

            _minBlinkTime = Time.realtimeSinceStartup + BlinkCooldown;
            int visibleLineCount = Mathf.FloorToInt(_imguiContainer.contentRect.height / LineHeight);
            int maxVisibleLine = Mathf.FloorToInt(_currentScrollValue + visibleLineCount);

            if (CursorY < _currentScrollValue) {
                _verticalScroller.value = CursorY;
            } else if (CursorY > maxVisibleLine) {
                _verticalScroller.value = CursorY - visibleLineCount + 1;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            UpdateScrollBar();
        }

        #endregion

        #region Modification Commands

        private void DeleteSelection() {
            if (ViewModel == null) return;

            Vector2Int min = SelectionMin;
            Vector2Int max = SelectionMax;

            ViewModel.DeleteText(min, max);
            CursorPosition = _selectionEnd = _selectionStart = min;
            _selectionActive = false;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void DeleteCharBeforeCursor() {
            if (ViewModel == null) return;
            Vector2Int deleteMin = CursorPosition;
            Vector2Int deleteMax = CursorPosition;

            if (CursorX > 0) {
                deleteMin.x--;
            } else if (CursorY > 0) {
                deleteMin = new Vector2Int(Lines[deleteMin.y - 1].Length, deleteMin.y - 1);
            }

            ViewModel.DeleteText(deleteMin, deleteMax);
            CursorPosition = _selectionEnd = _selectionStart = deleteMin;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void DeleteCharAfterCursor() {
            if (ViewModel == null) return;
            Vector2Int deleteMin = CursorPosition;
            Vector2Int deleteMax = CursorPosition;

            if (CursorX < Lines[CursorY].Length) {
                deleteMax.x++;
            } else if (CursorY > 0) {
                deleteMax = new Vector2Int(0, deleteMin.y + 1);
            }

            ViewModel.DeleteText(deleteMin, deleteMax);
            CursorPosition = _selectionEnd = _selectionStart = deleteMin;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertNewLine() {
            ViewModel.InsertNewLine(CursorPosition);
            CursorPosition = new Vector2Int(0, CursorY + 1);
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertText(string text) {
            ViewModel.InsertText(CursorPosition, text);
            CursorX += text.Length;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void InsertTextBeforeSelectedLines(string text) {
            int min = SelectionMin.y;
            int max = SelectionMax.y;

            for (int i = min; i <= max && i < Lines.Count; i++) {
                ViewModel.InsertText(new Vector2Int(0, i), text);
            }

            CursorX += text.Length;
            _selectionStart.x += text.Length;
            _selectionEnd.x += text.Length;
            _imguiContainer.MarkDirtyRepaint();
        }

        private void RemoveTextBeforeSelectedLines(string text) {
            int min = SelectionMin.y;
            int max = SelectionMax.y;

            IReadOnlyList<string> lines = Lines;

            for (int i = min; i <= max && i < lines.Count; i++) {
                int j;
                string line = lines[i];
                for (j = 0; j < text.Length && j < line.Length; j++) {
                    if (line[j] != text[j]) break;
                }

                ViewModel.DeleteText(new Vector2Int(0, i), new Vector2Int(j, i));

                if (i == _selectionStart.y) _selectionStart.x -= j;
                if (i == _selectionEnd.y) _selectionEnd.x -= j;
                if (i == CursorY) CursorX -= j;
            }

            _imguiContainer.MarkDirtyRepaint();
        }

        private void RemoveTextBeforeCursor(string text) {
            IReadOnlyList<string> lines = Lines;
            string line = lines[CursorY];

            int i;
            for (i = 0; i < text.Length && i < CursorX; i++) {
                int x = CursorX - (i + 1);
                if (line[x] != text[^(i + 1)]) break;
            }

            ViewModel.DeleteText(new Vector2Int(CursorX - i, CursorY), CursorPosition);
            CursorX -= i;
        }

        #endregion

        #region Scroll Handling

        private Vector2Int GetCursorPosition(Vector2 localMousePosition) {
            IReadOnlyList<string> lines = Lines;
            if (lines == null) return Vector2Int.zero;
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

        #region Drawing

        private void OnGui() {
            IReadOnlyList<string> lines = Lines;
            if (lines == null) return;

            GUISkin oldSkin = GUI.skin;
            GUI.skin = GuiSkin;

            try {
                Rect r = _imguiContainer.contentRect;

                int minVisibleLine =
                    Mathf.Clamp(Mathf.FloorToInt(_currentScrollValue), 0, lines.Count);
                int maxVisibleLine =
                    Mathf.Clamp(Mathf.CeilToInt(_currentScrollValue + r.height / LineHeight), 0, lines.Count);

                for (int i = minVisibleLine; i < maxVisibleLine; i++) {
                    float y = i - _currentScrollValue;
                    Rect rect = new(r.x, r.y + y * LineHeight, r.width, LineHeight);
                    DrawLineBg(i, rect);

                    Rect lineNumberRect = new(rect.x, rect.y, LineNumberWidth, LineHeight);
                    DrawLineNumber(lineNumberRect, i);

                    Rect lineRect = rect;
                    lineRect.xMin = lineNumberRect.xMax;

                    if (_selectionActive) {
                        DrawSelectionRect(i, lineRect, lines[i].Length);
                    }

                    DrawTextLine(lineRect, DisplayLines[i]);
                }

                if (_cursorActive) {
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
            Vector2Int selectionMin = SelectionMin;
            Vector2Int selectionMax = SelectionMax;

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
            float cursorX = LineNumberWidth + offset.x + CursorX * _charWidth;
            float cursorY = offset.y + CursorY * LineHeight;

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
