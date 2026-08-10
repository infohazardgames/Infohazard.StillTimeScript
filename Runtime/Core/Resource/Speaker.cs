using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Speaker : Resource {
        public StsColor Color { get; }
        public string Text { get; }

        public Speaker(string identifier, StsColor color, string text) : base(identifier) {
            Color = color;
            Text = text;
        }
    }
}
