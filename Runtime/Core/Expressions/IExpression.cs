using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public interface IExpression {
        public StsValueType Type { get; }
        
        public StsValue Evaluate(StateContainer state);
    }
}