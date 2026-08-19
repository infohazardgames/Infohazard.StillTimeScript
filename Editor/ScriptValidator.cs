using System;
using System.Collections.Generic;
using System.IO;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.Game;
using Infohazard.StillTimeScript.Game.Utility;
using UnityEditor;
using UnityEngine;

namespace StillTime.Editor {
    [InitializeOnLoad]
    public static class ScriptValidator {
        private static readonly Dictionary<string, DateTime> LastValidatedFileDateTimes = new();

        static ScriptValidator() {
            StsLibraryConfiguration.Run();
            EditorApplication.focusChanged += HandleFocusChanged;

            try {
                ValidateScripts(false);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }

            string[] ext = EditorSettings.projectGenerationUserExtensions;
            if (Array.IndexOf(ext, "sts") < 0) {
                Array.Resize(ref ext, ext.Length + 1);
                ext[^1] = "sts";
                EditorSettings.projectGenerationUserExtensions = ext;
            }
        }

        private static void HandleFocusChanged(bool value) {
            if (!value) return;

            try {
                ValidateScripts(false);
            }  catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MenuItem("Sts/Validate")]
        public static void ForceValidate() {
            ValidateScripts(true);
        }

        private static void ValidateScripts(bool force) {
            foreach (string scriptPath in Directory.GetFiles(Application.streamingAssetsPath, "*.sts", SearchOption.AllDirectories)) {
                DateTime lastModifiedTime = File.GetLastWriteTime(scriptPath);

                if (!force &&
                    LastValidatedFileDateTimes.TryGetValue(scriptPath, out DateTime lastValidateTime) &&
                    lastValidateTime >= lastModifiedTime)
                    continue;

                try {
                    ValidateScript(scriptPath);
                    if (force) Debug.Log($"Script <color=green>{scriptPath}</color> validated clean.");
                } catch (Exception ex) {
                    Debug.LogException(ex);
                }

                LastValidatedFileDateTimes[scriptPath] = lastModifiedTime;
            }
        }

        private static void ValidateScript(string path) {
            try {
                string scriptText = File.ReadAllText(path);
                List<ICommand> commands = ScriptParser.ParseScript(scriptText);
                GameGraph graph = GraphBuilder.BuildGraph(commands);
                graph.Validate();
            } catch (ParsingException ex) {
                string relativePath = Path.GetRelativePath(".", path);
                int line = ex.LineNumber + 1;
                Debug.LogError($"<a href=\"{relativePath}\" line=\"{line}\">{relativePath}</a>: {ex}");
            }
        }
    }
}
