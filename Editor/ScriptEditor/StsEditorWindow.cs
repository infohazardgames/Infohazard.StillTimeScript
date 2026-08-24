using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StillTime.Editor.ScriptEditor
{
    public class StsEditorWindow : EditorWindow {
        [SerializeField]
        private string _scriptPath;

        [SerializeField]
        private VisualTreeAsset _visualTree;

        private VisualElement _editorRootElement;
        private ToolbarMenu _fileMenu;
        private ToolbarMenu _editMenu;
        private ToolbarButton _backButton;
        private ToolbarButton _forwardButton;

        private StsEditorTextArea _textArea;

        [MenuItem("Sts/Open Editor")]
        public static void ShowWindow() {
            DefaultAsset scriptAsset = Selection.activeObject as DefaultAsset;
            if (!scriptAsset) {
                EditorUtility.DisplayDialog("Error", "No script selected", "OK");
                return;
            }

            string path = AssetDatabase.GetAssetPath(scriptAsset);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".sts", StringComparison.OrdinalIgnoreCase)) {
                EditorUtility.DisplayDialog("Error", "Invalid script path", "OK");
                return;
            }

            StsEditorWindow window = CreateInstance<StsEditorWindow>();
            window._scriptPath = path;
            window.titleContent = new GUIContent(Path.GetFileName(path));
            window.Show();
        }

        private void CreateGUI() {
            VisualElement treeElement = _visualTree.Instantiate();
            treeElement.style.flexGrow = 1;
            rootVisualElement.Add(treeElement);
            _editorRootElement = treeElement;
            _textArea = _editorRootElement.Q<StsEditorTextArea>();
            _fileMenu = _editorRootElement.Q<ToolbarMenu>("FileMenu");
            _editMenu = _editorRootElement.Q<ToolbarMenu>("EditMenu");
            _backButton = _editorRootElement.Q<ToolbarButton>("BackButton");
            _forwardButton = _editorRootElement.Q<ToolbarButton>("ForwardButton");

            string[] lines = File.ReadAllLines(_scriptPath);
            _textArea.ViewModel.Rebuild(lines);
            _textArea.ViewModel.IsModifiedChanged += OnIsModifiedChanged;

            _editorRootElement.RegisterCallback<KeyDownEvent>(OnKeyDown, CallbackOptions.TrickleDown);
            SetupFileMenu();
            SetupEditMenu();
            _backButton.clicked += _textArea.GoBack;
            _forwardButton.clicked += _textArea.GoForward;
        }
        
        private void SetupFileMenu() {
            _fileMenu.SetEnabled(true);
            _fileMenu.menu.AppendAction("Save", _ => SaveFile(), _ => _textArea.ViewModel.IsModified
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            _fileMenu.menu.AppendAction("Save As", _ => SaveFileAs());
        }

        private void SetupEditMenu() {
            _editMenu.SetEnabled(true);
            _editMenu.menu.AppendAction("Undo", _ => _textArea.ActionStack.Undo(), _ => _textArea.ActionStack.CanUndo
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            _editMenu.menu.AppendAction("Redo", _ => _textArea.ActionStack.Redo(), _ => _textArea.ActionStack.CanRedo
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            _editMenu.menu.AppendSeparator();
            _editMenu.menu.AppendAction("Cut", _ => _textArea.CutSelection(), _ => _textArea.ViewModel.SelectionActive
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            _editMenu.menu.AppendAction("Copy", _ => _textArea.CopySelection(), _ => _textArea.ViewModel.SelectionActive
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            _editMenu.menu.AppendAction("Paste", _ => _textArea.PasteClipboard(), _ =>
                GUIUtility.systemCopyBuffer.Length > 0 && _textArea.ViewModel.CursorActive
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);
        }

        private void OnIsModifiedChanged(bool isModified) {
            titleContent = new GUIContent(Path.GetFileName(_scriptPath) + (isModified ? "*" : ""));
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.ctrlKey) {
                if (evt.keyCode == KeyCode.S) {
                    SaveFile();
                    evt.StopPropagation();
                }
            }
        }

        private void SaveFile() {
            if (!_textArea.ViewModel.IsModified) return;
            File.WriteAllLines(_scriptPath, _textArea.ViewModel.ScriptLines);
            _textArea.ViewModel.ClearModified();
        }

        private void SaveFileAs() {
            string path = EditorUtility.SaveFilePanelInProject("Save File",
                Path.GetFileNameWithoutExtension(_scriptPath), "sts", "", Path.GetDirectoryName(_scriptPath));
            if (path == null) return;
            
            _scriptPath = path;
            titleContent = new GUIContent(Path.GetFileName(_scriptPath));
            File.WriteAllLines(_scriptPath, _textArea.ViewModel.ScriptLines);
            _textArea.ViewModel.ClearModified();
        }
    }
}
