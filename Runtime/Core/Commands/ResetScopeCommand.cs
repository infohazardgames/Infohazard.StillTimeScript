using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("reset", 1)]
    public class ResetScopeCommand : Command, ISequentialCommand {
        public string Scope { get; }

        public ResetScopeCommand(LineTokens tokens) : base(tokens) {
            Scope = tokens.GetArg(0);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            Scope scope = graphData.GetResource<Scope>(this, Scope);
            ResetScopeNode node = new(scope);
            builder.Append(node);
        }
    }
}
