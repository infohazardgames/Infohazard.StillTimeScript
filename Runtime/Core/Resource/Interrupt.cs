using Infohazard.StillTimeScript.Core.Expressions;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Interrupt : Resource {
        public IExpression Target { get; set; }
        public IExpression Condition { get; set; }

        public Interrupt(string identifier) : base(identifier) { }
    }
}
