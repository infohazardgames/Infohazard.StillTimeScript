using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class ErrorAnnotation : LineAnnotation {
        public string Message { get; }

        public ErrorAnnotation(StsRange range, string message) : base(range) {
            Message = message;
        }
    }
}
