using System;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.Runner;
using Infohazard.StillTimeScript.Game.Utility;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infohazard.StillTimeScript.Game.View {
    public class SettingsMenu : MonoBehaviour {
        public Toggle _skipToggle;
        public Toggle _skipSeenToggle;
        public GameRunner _gameRunner;
        public GameSettings _gameSettings;
        public TMP_InputField _jumpInput;

        private void OnEnable() {
            _skipToggle.isOn = _gameSettings.SkipAnimations;
            _skipSeenToggle.isOn = _gameSettings.SkipSeenDialogue;
        }

        public void ToggleAnimations(bool value) {
            _gameSettings.SkipAnimations = value;
        }

        public void ToggleSkipSeen(bool value) {
            _gameSettings.SkipSeenDialogue = value;
        }

        public void ResetGame() {
            _gameRunner.ClearGameState();
            _gameRunner.StartNewGame();
        }

        public void DoJump() {
            string text = _jumpInput.text;
            if (string.IsNullOrEmpty(text)) return;

            JToken prevState = _gameRunner.SaveGame();
            try {
                JToken modState = prevState.DeepClone();
                modState[nameof(CurrentNodeComponent)]![nameof(CurrentNodeComponent.CurrentNode)] = text;
                _gameRunner.ClearGameState();
                _gameRunner.LoadGame(modState);
            } catch (Exception ex) {
                Debug.LogException(ex);
                _gameRunner.LoadGame(prevState);
            }
        }
    }
}
