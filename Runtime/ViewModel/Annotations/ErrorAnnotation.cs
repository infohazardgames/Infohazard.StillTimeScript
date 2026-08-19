using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class ErrorAnnotation : LineAnnotation {
        public string Message { get; }

        public ErrorAnnotation(int lineNumber, StsRange range, string message) : base(lineNumber, range) {
            Message = message;
        }
    }
}
