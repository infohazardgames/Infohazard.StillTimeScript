using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Utility {
    public class GameSettings : MonoBehaviour {
        private const string SkipAnimationsKey = "StillTime.SkipAnimations";
        private const string SkipSeenDialogKey = "StillTime.SkipSeenDialog";
        private bool _skipAnimations;
        private bool _skipSeenDialogue;

        public bool SkipAnimations {
            get => _skipAnimations;
            set {
                if (_skipAnimations == value) return;
                _skipAnimations = value;
                PlayerPrefs.SetInt(SkipAnimationsKey, value ? 1 : 0);
            }
        }

        public bool SkipSeenDialogue {
            get => _skipSeenDialogue;
            set {
                if (_skipSeenDialogue == value) return;
                _skipSeenDialogue = value;
                PlayerPrefs.SetInt(SkipSeenDialogKey, value ? 1 : 0);
            }
        }

        private void OnEnable() {
            _skipAnimations = PlayerPrefs.GetInt(SkipAnimationsKey, 0) != 0;
            _skipSeenDialogue = PlayerPrefs.GetInt(SkipSeenDialogKey, 1) != 0;
        }
    }
}
