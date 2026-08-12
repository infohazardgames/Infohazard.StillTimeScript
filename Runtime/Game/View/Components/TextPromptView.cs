using System;
using System.Collections.Generic;
using Infohazard.Core;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infohazard.StillTimeScript.Game.View.Components {
    public class TextPromptView : GameViewComponent {
        public GameObject _mainPanel;
        public TMP_Text _mainText;
        public Button _mainButton;

        public ChoiceView _choiceViewPrefab;
        public Transform _choiceViewParent;

        public PassiveTimer _showWordTimer;
        public PassiveTimer _showButtonTimer;

        private Action _mainButtonAction;
        private int _visibleButtonCount;
        private readonly List<ChoiceView> _currentChoices = new();
        private TMP_TextInfo _textInfo;
        private bool _autoAdvance;

        public event Action Cancellation;

        private void OnEnable() {
            Clear(true);

            _mainButton.onClick.RemoveListener(HandleButton);
            _mainButton.onClick.AddListener(HandleButton);

            _showWordTimer.Initialize();
            _showButtonTimer.Initialize();
        }

        private void OnDisable() {
            _mainButton.onClick.RemoveListener(HandleButton);
            Cancellation?.Invoke();
        }

        private void SetText(string text) {
            _mainText.text = text;
            _textInfo = _mainText.GetTextInfo(text);
        }

        private void SkipAnimation() {
            _mainText.maxVisibleWords = _textInfo?.wordCount ?? 0 + 1;
            for (int i = 0; i < _currentChoices.Count; i++) {
                _currentChoices[i].transform.localScale = Vector3.one;
            }

            _visibleButtonCount = _currentChoices.Count;
            _mainButton.gameObject.SetActive(_currentChoices.Count == 0);
        }

        private void HandleButton() {
            if (_mainText.maxVisibleWords < _textInfo?.wordCount + 1 ||
                _visibleButtonCount < _currentChoices.Count) {
                SkipAnimation();
            } else {
                _mainButtonAction?.Invoke();
            }
        }

        private string AddSpeakerToText(string text, Speaker speaker, StateContainer state) {
            if (speaker == null) return text;

            string speakerText = speaker.Text.Evaluate(state).StringValue;
            if (speaker.Color != null) {
                StsColor color = speaker.Color.Evaluate(state).ColorValue;
                speakerText = $"<color=#{color.ToHexString()}>{speakerText}</color>\n{text}";
            }

            return speakerText;
        }

        public void SetSingleText(
            string text,
            Speaker speaker,
            StateContainer state,
            Action next,
            bool skipAnimation,
            bool autoAdvance) {

            Clear();

            SetText(AddSpeakerToText(text, speaker, state));
            _mainButtonAction = next;

            if (skipAnimation) {
                SkipAnimation();
            }

            _autoAdvance = autoAdvance;
            _showWordTimer.StartInterval();
        }

        public void SetChoices(
            string mainText,
            Speaker speaker,
            StateContainer state,
            List<(string text, Action action, bool hasNew)> choices,
            bool skipAnimation) {

            Clear();

            SetText(AddSpeakerToText(mainText, speaker, state));
            foreach ((string text, Action action, bool hasNew) in choices) {
                ChoiceView choiceView = Instantiate(_choiceViewPrefab, _choiceViewParent, false);
                choiceView.Configure(text, action, hasNew);
                choiceView.transform.localScale = Vector3.zero;
                _currentChoices.Add(choiceView);
            }

            if (skipAnimation) {
                SkipAnimation();
            } else {
                _showWordTimer.StartInterval();
            }
        }

        private void Update() {
            if (_mainText.isActiveAndEnabled &&
                _mainText.text.Length > 0 &&
                _textInfo != null &&
                _mainText.maxVisibleWords < _textInfo.wordCount + 1 &&
                _showWordTimer.TryConsume()) {
                _mainText.maxVisibleWords++;
                _showButtonTimer.StartInterval();
            }

            if (_textInfo != null &&
                _mainText.maxVisibleWords >= _textInfo.wordCount + 1 &&
                _currentChoices.Count > 0 &&
                _visibleButtonCount < _currentChoices.Count) {

                ChoiceView currentButton = _currentChoices[_visibleButtonCount];
                float ratio = _showButtonTimer.RatioSinceIntervalStart;
                currentButton.transform.localScale = Vector3.one * ratio;

                if (ratio == 1) {
                    _showButtonTimer.StartInterval();
                    _visibleButtonCount++;

                    if (_visibleButtonCount == _currentChoices.Count) {
                        _mainButton.gameObject.SetActive(false);
                    }
                }
            }

            if (_textInfo != null &&
                _mainText.maxVisibleWords >= _textInfo.wordCount + 1 &&
                _currentChoices.Count == 0 &&
                _autoAdvance &&
                _mainButtonAction != null &&
                _showWordTimer.TryConsume()) {
                _mainButtonAction();
            }
        }

        public override void Clear() {
            Clear(true);
        }

        private void Clear(bool hidePanel = false) {
            _choiceViewParent.DestroyChildren();
            _mainButtonAction = null;
            SetText(string.Empty);
            _mainText.maxVisibleWords = 0;
            _mainButton.gameObject.SetActive(!hidePanel);
            _visibleButtonCount = 0;
            _currentChoices.Clear();
            _mainPanel.SetActive(!hidePanel);
            Cancellation?.Invoke();
        }
    }
}
