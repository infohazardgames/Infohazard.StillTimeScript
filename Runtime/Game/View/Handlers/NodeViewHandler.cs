using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public abstract class NodeViewHandler : MonoBehaviour {
        public abstract IEnumerable<Type> HandledNodeTypes { get; }

        public abstract UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            CancellationToken cancellationToken);

        public virtual void HandleInitialState(GameGraph graph, StateContainer state) { }
    }

    public abstract class NodeViewHandler<T> : NodeViewHandler where T : INode {
        public override IEnumerable<Type> HandledNodeTypes { get; } = new[] { typeof(T) };

        public override UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            CancellationToken cancellationToken) {
            if (state.GetOrCreate<CurrentNodeComponent>().CurrentNode is not T typedNode) {
                throw new InvalidOperationException($"Invalid node passed to NodeViewHandler<{typeof(T)}>");
            }

            return HandleState(graph, state, typedNode, cancellationToken);
        }

        protected abstract UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            T node,
            CancellationToken cancellationToken);
    }
}
