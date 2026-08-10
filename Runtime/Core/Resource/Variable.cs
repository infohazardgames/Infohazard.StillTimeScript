using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Resource {
    public class Variable : Resource {
        public StsValueType Type { get; }

        public string ScopeId { get; }

        public StsValue DefaultValue { get; }

        public Variable(string identifier, StsValueType type, string scopeId, StsValue defaultValue) : base(identifier) {
            Type = type;
            ScopeId = scopeId;
            DefaultValue = defaultValue;
        }
    }
}
