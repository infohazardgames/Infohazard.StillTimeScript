using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class ScopeCommand : Command, IResourceCommand {
        public string Identifier { get; }

        public ScopeCommand(int lineNumber, string line, string identifier) : base(lineNumber, line) {
            Identifier = identifier;
        }

        public void CreateResources(GraphData graphData) {
            Scope scope = new(Identifier);
            graphData.Resources.Add(Identifier, scope);
        }
    }
}
