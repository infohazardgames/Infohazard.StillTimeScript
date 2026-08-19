using System;
using System.IO;
using UnityEditor;
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

            string[] lines = File.ReadAllLines(_scriptPath);
            _textArea.ViewModel.Rebuild(lines);
            _textArea.ViewModel.IsModifiedChanged += OnIsModifiedChanged;

            _editorRootElement.RegisterCallback<KeyDownEvent>(OnKeyDown, CallbackOptions.TrickleDown);
        }

        private void OnIsModifiedChanged(bool isModified) {
            titleContent = new GUIContent(Path.GetFileName(_scriptPath) + (isModified ? "*" : ""));
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.ctrlKey) {
                if (evt.keyCode == KeyCode.S) {
                    if (_textArea.ViewModel.IsModified) {
                        File.WriteAllLines(_scriptPath, _textArea.ViewModel.ScriptLines);
                        _textArea.ViewModel.ClearModified();
                    }

                    evt.StopPropagation();
                }
            }
        }
    }
}
