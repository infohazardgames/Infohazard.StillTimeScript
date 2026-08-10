using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ResetScopeCommand : Command, ISequentialCommand {
        public string Scope { get; }

        public ResetScopeCommand(int lineNumber, string line, string scope) : base(lineNumber, line) {
            Scope = scope;
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Scope scope = graphData.GetResource<Scope>(this, Scope);
            ResetScopeNode node = new(scope);
            builder.Append(node);
        }
    }
}
