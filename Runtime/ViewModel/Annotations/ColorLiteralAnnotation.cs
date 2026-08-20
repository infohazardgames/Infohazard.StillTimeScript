using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class ColorLiteralAnnotation : LineAnnotation {
        public StsColor Color { get; }

        public ColorLiteralAnnotation(StsRange range, StsColor color) : base(range) {
            Color = color;
        }
    }
}
