using Infohazard.Core;
using Infohazard.StillTimeScript.Core.Utility;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Components {
    public class BackgroundView : GameViewComponent {
        public Camera _camera;
        private PassiveTimer _timer;
        private Color _sourceColor;
        private Color _destColor;

        public void SetColor(StsColor color, float time) {
            Color unityColor = new(color.R, color.G, color.B, color.A);
            if (time == 0) {
                _camera.backgroundColor = unityColor;
                _sourceColor = _destColor = unityColor;
                _timer.EndInterval();
            } else {
                _sourceColor = _camera.backgroundColor;
                _destColor = unityColor;
                _timer.Interval = time;
                _timer.StartInterval();
            }
        }

        public override void Clear() {
            base.Clear();

            _camera.backgroundColor = Color.clear;
            _sourceColor = _destColor = Color.clear;
            _timer.EndInterval();
        }

        private void Update() {
            if (!_timer.IsIntervalEnded) {
                _camera.backgroundColor = Color.Lerp(_sourceColor, _destColor, _timer.RatioSinceIntervalStart);
            } else if (_timer.DidIntervalEndThisFrame) {
                _camera.backgroundColor = _destColor;
            }
        }
    }
}
