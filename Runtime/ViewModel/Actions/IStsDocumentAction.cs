#nullable enable

namespace Infohazard.StillTimeScript.ViewModel.Actions {
    public interface IStsDocumentAction {
        public void Execute(StsDocumentViewModel viewModel);

        public void Undo(StsDocumentViewModel viewModel);

        public void Redo(StsDocumentViewModel viewModel) {
            Execute(viewModel);
        }
    }
}
