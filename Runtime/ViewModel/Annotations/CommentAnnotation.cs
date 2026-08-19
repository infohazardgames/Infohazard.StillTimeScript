namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class CommentAnnotation : LineAnnotation {
        public override string StartText => "<color=green>";

        public override string EndText => "</color>";

        public CommentAnnotation(int lineNumber, int rangeStart, int rangeEnd) :
            base(lineNumber, rangeStart, rangeEnd) { }
    }
}
