using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("scope", 1)]
    public class ScopeCommand : Command, IResourceCommand {
        public string Identifier { get; }

        public ScopeCommand(LineTokens tokens) : base(tokens) {
            Identifier = tokens.GetArg(0);
        }

        public void CreateResources(GraphData graphData) {
            Scope scope = new(Identifier);
            graphData.Resources.Add(Identifier, scope);
        }
    }
}
