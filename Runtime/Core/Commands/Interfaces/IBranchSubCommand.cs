using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands.Interfaces {
    public interface IBranchSubCommand : ICommand {
        public void CreateBranchOptions(GraphData graphData, List<IBranchOption> options);
    }
}
