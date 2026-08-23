#nullable enable

using System.Collections.Generic;
using Infohazard.StillTimeScript.ViewModel.Data;

namespace Infohazard.StillTimeScript.ViewModel.Actions {
    public class CompoundAction : IStsDocumentAction {
        private readonly List<IStsDocumentAction> _subActions = new();

        public StsCursorRange? OriginalSelection { get; set; }
        public StsCursorPos? OriginalCursorPosition { get; set; }
        public StsCursorRange? NewSelection { get; set; }
        public StsCursorPos? NewCursorPosition { get; set; }

        public void AddAction(IStsDocumentAction action) {
            _subActions.Add(action);
        }

        public void Execute(StsDocumentViewModel viewModel) {
            using (viewModel.DeferUpdates()) {
                for (int i = 0; i < _subActions.Count; i++) {
                    _subActions[i].Execute(viewModel);
                }
            }

            if (NewSelection.HasValue) viewModel.Selection = NewSelection.Value;
            if (NewCursorPosition.HasValue) viewModel.CursorPosition = NewCursorPosition.Value;
        }

        public void Undo(StsDocumentViewModel viewModel) {
            using (viewModel.DeferUpdates()) {
                for (int i = _subActions.Count - 1; i >= 0; i--) {
                    _subActions[i].Undo(viewModel);
                }
            }

            if (OriginalSelection.HasValue) viewModel.Selection = OriginalSelection.Value;
            if (OriginalCursorPosition.HasValue) viewModel.CursorPosition = OriginalCursorPosition.Value;
        }
    }
}
