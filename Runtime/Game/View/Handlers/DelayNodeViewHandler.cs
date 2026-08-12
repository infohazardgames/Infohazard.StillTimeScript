using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public class DelayNodeViewHandler : NodeViewHandler<DelayNode> {
        protected override async UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            DelayNode node,
            CancellationToken cancellationToken) {

            float time = (float) node.Time.Evaluate(state).NumberValue;
            await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: cancellationToken);
            return node.Next;
        }
    }
}
