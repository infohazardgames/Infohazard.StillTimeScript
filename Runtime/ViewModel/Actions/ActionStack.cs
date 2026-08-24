using System.Collections.Generic;

namespace Infohazard.StillTimeScript.ViewModel.Actions {
    public class ActionStack {
        private readonly StsDocumentViewModel _viewModel;

        private readonly List<IStsDocumentAction> _undoStack = new();
        private readonly List<IStsDocumentAction> _redoStack = new();
        
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public ActionStack(StsDocumentViewModel viewModel) {
            _viewModel = viewModel;
        }

        public void ExecuteAction(IStsDocumentAction action) {
            action.Execute(_viewModel);
            _undoStack.Add(action);
            _redoStack.Clear();
        }

        public bool Undo() {
            if (_undoStack.Count == 0) return false;

            IStsDocumentAction action = _undoStack[^1];
            action.Undo(_viewModel);
            _redoStack.Add(action);
            _undoStack.RemoveAt(_undoStack.Count - 1);

            return true;
        }

        public bool Redo() {
            if (_redoStack.Count == 0) return false;

            IStsDocumentAction action = _redoStack[^1];
            action.Redo(_viewModel);
            _undoStack.Add(action);
            _redoStack.RemoveAt(_redoStack.Count - 1);

            return true;
        }
    }
}
