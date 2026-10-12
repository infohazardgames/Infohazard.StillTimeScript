using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.ViewModel;
using UnityEngine.UIElements;

namespace StillTime.Editor.ScriptEditor {
    [UxmlElement]
    public partial class StsEditorFindBar : VisualElement {
        private StsDocumentFindReplaceViewModel _viewModel;
        private bool _viewDirty;

        public VisualElement FindRow { get; }
        public VisualElement ReplaceRow { get; }
        public VisualElement Spacer { get; }

        public TextField FindField { get; }
        public TextField ReplaceField { get; }

        public Button FindNextButton { get; }
        public Button FindPreviousButton { get; }
        public Button ReplaceButton { get; }
        public Button ReplaceAllButton { get; }
        public Button CloseButton { get; }

        public StsDocumentFindReplaceViewModel ViewModel {
            get => _viewModel;
            set {
                if (_viewModel == value) return;

                if (_viewModel != null) {
                    _viewModel.Updated -= HandleViewModelUpdated;
                }

                _viewModel = value;

                if (_viewModel != null) {
                    _viewModel.Updated += HandleViewModelUpdated;
                }

                UpdateView();
            }
        }

        public bool Show {
            get => style.display == DisplayStyle.Flex;
            set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public StsEditorFindBar() {
            AddToClassList("sts-find-bar");

            FindRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            FindRow.AddToClassList("sts-find-bar__find-row");
            ReplaceRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            ReplaceRow.AddToClassList("sts-find-bar__replace-row");

            Add(FindRow);
            Add(ReplaceRow);

            FindField = new TextField { textEdition = { placeholder = "Find in file..." } };
            FindField.AddToClassList("sts-find-bar__find-field");
            FindNextButton = new Button { text = "↓" };
            FindNextButton.AddToClassList("sts-find-bar__find-next-button");
            FindPreviousButton = new Button { text = "↑" };
            FindPreviousButton.AddToClassList("sts-find-bar__find-previous-button");
            Spacer = new VisualElement { style = { flexGrow = 1 } };
            CloseButton = new Button { text = "X" };
            CloseButton.AddToClassList("sts-find-bar__close-button");
            FindRow.Add(FindField);
            FindRow.Add(FindNextButton);
            FindRow.Add(FindPreviousButton);
            FindRow.Add(Spacer);
            FindRow.Add(CloseButton);

            ReplaceField = new TextField { textEdition = { placeholder = "Replace with..." } };
            ReplaceField.AddToClassList("sts-find-bar__replace-field");
            ReplaceButton = new Button { text = "Replace" };
            ReplaceButton.AddToClassList("sts-find-bar__replace-button");
            ReplaceAllButton = new Button { text = "Replace All" };
            ReplaceAllButton.AddToClassList("sts-find-bar__replace-all-button");
            ReplaceRow.Add(ReplaceField);
            ReplaceRow.Add(ReplaceButton);
            ReplaceRow.Add(ReplaceAllButton);

            FindNextButton.clicked += HandleFindNextButtonClicked;
            FindPreviousButton.clicked += HandleFindPreviousButtonClicked;
            ReplaceButton.clicked += HandleReplaceButtonClicked;
            ReplaceAllButton.clicked += HandleReplaceAllButtonClicked;

            FindField.RegisterValueChangedCallback(HandleFindFieldChanged);
            ReplaceField.RegisterValueChangedCallback(HandleReplaceFieldChanged);

            CloseButton.clicked += () => Show = false;

            Show = false;
            UpdateView();
        }

        private void HandleFindNextButtonClicked() {
            if (ViewModel is not { Occurrences: { Length: > 0 } })
                return;

            ViewModel.DoFindNext();
        }

        private void HandleFindPreviousButtonClicked() {
            if (ViewModel is not { Occurrences: { Length: > 0 } })
                return;

            ViewModel.DoFindPrevious();
        }

        private void HandleReplaceButtonClicked() {
            if (ViewModel is not { Occurrences: { Length: > 0 } })
                return;

            ViewModel.DoReplace();
        }

        private void HandleReplaceAllButtonClicked() {
            if (ViewModel is not { Occurrences: { Length: > 0 } })
                return;

            ViewModel.DoReplaceAll();
        }

        private void HandleFindFieldChanged(ChangeEvent<string> evt) {
            if (ViewModel == null)
                return;

            ViewModel.FindText = evt.newValue;
        }

        private void HandleReplaceFieldChanged(ChangeEvent<string> evt) {
            if (ViewModel == null)
                return;

            ViewModel.ReplaceText = evt.newValue;
        }

        private void HandleViewModelUpdated() {
            if (_viewDirty) return;

            _viewDirty = true;
            UniTask.NextFrame().ContinueWith(() => {
                _viewDirty = false;
                UpdateView();
            });
        }

        private void UpdateView() {
            bool textExists = ViewModel is { Occurrences: { Length: > 0 } };

            FindNextButton.enabledSelf = textExists;
            FindPreviousButton.enabledSelf = textExists;
            ReplaceButton.enabledSelf = textExists;
            ReplaceAllButton.enabledSelf = textExists;
        }
    }
}
