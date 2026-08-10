using TMPro;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Components {
    public class CountdownView : GameViewComponent {
        public TMP_Text _countdownText;
        public GameObject _countdownObject;

        public void ShowCountdown(string text) {
            _countdownObject.SetActive(true);
            _countdownText.text = text;
        }

        public void HideCountdown() {
            _countdownObject.SetActive(false);
        }

        public override void Clear() {
            _countdownObject.SetActive(false);
        }
    }
}
