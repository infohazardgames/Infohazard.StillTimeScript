using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public abstract class LineAnnotation {
        public StsRange Range { get; }

        protected LineAnnotation(StsRange range) {
            Range = range;
        }
    }
}
