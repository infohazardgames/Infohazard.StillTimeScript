#nullable enable

using Infohazard.StillTimeScript.ViewModel.Data;

namespace Infohazard.StillTimeScript.ViewModel.Actions {
    public class ChangeText : IStsDocumentAction {
        private readonly StsCursorRange _originalRange;
        private readonly StsCursorRange _newRange;
        private readonly string? _originalText;
        private readonly string? _newText;

        public StsCursorRange? OriginalSelection { get; set; }
        public StsCursorPos? OriginalCursorPosition { get; set; }
        public StsCursorRange? NewSelection { get; set; }
        public StsCursorPos? NewCursorPosition { get; set; }

        private ChangeText(StsCursorRange originalRange, StsCursorRange newRange,
                           string? originalText, string? newText) {
            _originalRange = originalRange;
            _newRange = newRange;
            _originalText = originalText;
            _newText = newText;

            OriginalSelection = _originalRange;
            OriginalCursorPosition = _originalRange.End;
            NewSelection = new StsCursorRange(_newRange.Max, _newRange.Max);
            NewCursorPosition = _newRange.Max;
        }

        public void Execute(StsDocumentViewModel viewModel) {
            using (viewModel.DeferUpdates()) {
                if (_originalText != null) {
                    viewModel.DeleteText(_originalRange);
                }

                if (_newText != null) {
                    viewModel.InsertText(_newRange.Min, _newText);
                }
            }

            if (NewSelection.HasValue) viewModel.Selection = NewSelection.Value;
            if (NewCursorPosition.HasValue) viewModel.CursorPosition = NewCursorPosition.Value;
        }

        public void Undo(StsDocumentViewModel viewModel) {
            using (viewModel.DeferUpdates()) {
                if (_newText != null) {
                    viewModel.DeleteText(_newRange);
                }

                if (_originalText != null) {
                    viewModel.InsertText(_originalRange.Min, _originalText);
                }
            }

            if (OriginalSelection.HasValue) viewModel.Selection = OriginalSelection.Value;
            if (OriginalCursorPosition.HasValue) viewModel.CursorPosition = OriginalCursorPosition.Value;
        }

        public static ChangeText Insert(StsCursorPos start, string text) {
            StsCursorRange newRange = StsCursorRange.GetRangeOfText(start, text);
            return new ChangeText(new StsCursorRange(start, start), newRange, null, text);
        }

        public static ChangeText Delete(StsDocumentViewModel viewModel, StsCursorRange range) {
            string? deletedText = viewModel.GetText(range);
            return new ChangeText(range, new StsCursorRange(range.Min, range.Min), deletedText, null);
        }

        public static ChangeText Replace(StsDocumentViewModel viewModel, StsCursorRange range, string newText) {
            string? deletedText = viewModel.GetText(range);
            StsCursorRange newRange = StsCursorRange.GetRangeOfText(range.Min, newText);
            return new ChangeText(range, newRange, deletedText, newText);
        }
    }
}
