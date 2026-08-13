using Infohazard.StillTimeScript.Core.Expressions;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Speaker : Resource {
        public IExpression Color { get; set; }
        public IExpression Text { get; set; }

        public Speaker(string identifier) : base(identifier) { }
    }
}
