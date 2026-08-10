using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Interrupt : Resource {
        public INode TargetNode { get; set; }
        public IExpression Condition { get; set; }
        
        public Interrupt(string identifier) : base(identifier) { }
    }
}