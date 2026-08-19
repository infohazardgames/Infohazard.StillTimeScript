using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class CommentAnnotation : LineAnnotation {
        public CommentAnnotation(int lineNumber, StsRange range) : base(lineNumber, range) { }
    }
}
