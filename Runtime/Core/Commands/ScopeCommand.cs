using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("scope", 1)]
    public class ScopeCommand : Command, IResourceCommand {
        public Token Identifier { get; }

        public ScopeCommand(LineTokens tokens) : base(tokens) {
            Identifier = tokens.GetRequiredArg(0);
        }

        public void CreateResources(GraphData graphData) {
            Scope scope = new(Identifier.Text);
            graphData.Resources.Add(Identifier.Text, scope);
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            yield return new CommandToken(Identifier, CommandTokenType.Definition);
        }
    }
}
