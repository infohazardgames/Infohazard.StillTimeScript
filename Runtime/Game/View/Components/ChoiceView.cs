using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infohazard.StillTimeScript.Game.View.Components {
    public class ChoiceView : MonoBehaviour {
        public Button _button;
        public TMP_Text _label;
        public Color _noNewColor;

        public void Configure(string text, Action action, bool hasNewContent) {
            _label.text = text;
            _button.onClick.AddListener(() => action());

            if (!hasNewContent) {
                _label.color = _noNewColor;
            }
        }
    }
}
