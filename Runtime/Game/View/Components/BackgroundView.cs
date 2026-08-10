using DG.Tweening;
using Infohazard.StillTimeScript.Core.Utility;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Components {
    public class BackgroundView : GameViewComponent {
        public Camera _camera;

        public void SetColor(StsColor color, float time) {
            _camera.DOKill();
            Color unityColor = new(color.R, color.G, color.B, color.A);
            if (time == 0) {
                _camera.backgroundColor = unityColor;
            } else {
                _camera.DOColor(unityColor, time);
            }
        }

        public override void Clear() {
            base.Clear();

            _camera.backgroundColor = Color.clear;
        }
    }
}
