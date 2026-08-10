using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public class ClearNodeViewHandler : NodeViewHandler<ClearNode> {
        public GameViewRoot _viewRoot;

        protected override UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            ClearNode node,
            CancellationToken cancellationToken) {

            _viewRoot.Clear();

            return UniTask.FromResult(node.Next);
        }
    }
}
