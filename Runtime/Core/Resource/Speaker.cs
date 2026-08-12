using Infohazard.StillTimeScript.Core.Expressions;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Speaker : Resource {
        public IExpression Color { get; }
        public IExpression Text { get; }

        public Speaker(string identifier, IExpression color, IExpression text) : base(identifier) {
            Color = color;
            Text = text;
        }
    }
}
