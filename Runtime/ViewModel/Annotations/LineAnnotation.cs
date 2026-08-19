
using System.Text;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public abstract class LineAnnotation {
        public int LineNumber { get; }
        public int RangeStart { get; }
        public int RangeEnd { get; }

        public abstract string StartText { get; }
        public abstract string EndText { get; }

        protected LineAnnotation(int lineNumber, int rangeStart, int rangeEnd) {
            LineNumber = lineNumber;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
        }
    }
}
