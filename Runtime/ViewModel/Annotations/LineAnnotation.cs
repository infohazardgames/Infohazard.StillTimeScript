
using System.Text;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public abstract class LineAnnotation {
        public int LineNumber { get; }
        public StsRange Range { get; }

        protected LineAnnotation(int lineNumber, StsRange range) {
            LineNumber = lineNumber;
            Range = range;
        }
    }
}
