using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.Game.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class EntryPoint : MonoBehaviour {
        public string _scriptPath;
        public GameRunner _gameRunner;

        public PassiveTimer _saveGameTimer;

        private const string SaveGameKey = "StillTime.SaveData";

        private void Start() {
            StsLibraryConfiguration.Run();
            LoadFromWebAsync().Forget();
        }

        private async UniTask LoadFromWebAsync() {
            using DownloadHandlerBuffer buffer = new();
            string scriptPath = Path.Combine(Application.streamingAssetsPath, _scriptPath);
            using UnityWebRequest request = new(new Uri(scriptPath), "GET");
            request.downloadHandler = buffer;

            try {
                await request.SendWebRequest();
                using StreamReader reader = new(new MemoryStream(buffer.data));
                LoadFromScriptText(reader.ReadToEnd());
                Debug.Log("Successfully loaded remote script.");
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private void LoadFromScriptText(string scriptText) {
            List<ICommand> commands = ScriptParser.ParseScript(scriptText);
            GameGraph graph = GraphBuilder.BuildGraph(commands);
            graph.Validate();

            _gameRunner.LoadGameGraph(graph);

            string saveData = PlayerPrefs.GetString(SaveGameKey, string.Empty);

            if (string.IsNullOrEmpty(saveData)) {
                _gameRunner.StartNewGame();
            } else {
                try {
                    JToken serializedState = JToken.Parse(saveData);
                    _gameRunner.LoadGame(serializedState);
                } catch (Exception ex) {
                    Debug.LogException(ex);
                    _gameRunner.StartNewGame();
                }
            }

            _saveGameTimer.Initialize();
        }

        private void OnDisable() {
            SaveGame();
        }

        private void Update() {
            if (_saveGameTimer.TryConsume()) {
                SaveGame();
            }
        }

        private void SaveGame() {
            JToken saveData = _gameRunner.SaveGame();
            string json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(SaveGameKey, json);
        }
    }
}
