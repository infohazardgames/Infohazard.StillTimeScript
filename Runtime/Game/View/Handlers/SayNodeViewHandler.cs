using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.Utility;
using Infohazard.StillTimeScript.Game.View.Components;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public class SayNodeViewHandler : NodeViewHandler<SayNode> {
        public TextPromptView _view;
        public GameSettings _gameSettings;
        public bool _alwaysSkip;
        public string _skipSeenScopeName;

        protected override async UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            SayNode node,
            CancellationToken cancellationToken) {

            graph.TryGetResource(_skipSeenScopeName, out Scope scope);

            UniTaskCompletionSource tcs = new();

            bool skipDialog =
                (_alwaysSkip &&
                 Application.isEditor) ||
                (scope != null &&
                 _gameSettings.SkipSeenDialogue &&
                 !state.GetOrCreate<VisitedNodesComponent>().WasCurrentStateUnexplored(scope));

            Action advance = () => tcs.TrySetResult();
            Action cancel = () => tcs.TrySetCanceled();

            cancellationToken.Register(cancel);

            _view.SetSingleText(
                GameUtility.DoStringInterpolation(node.Text, graph, state),
                node.Speaker,
                advance,
                _gameSettings.SkipAnimations,
                skipDialog);

            _view.Cancellation += cancel;

            await tcs.Task;
            _view.Cancellation -= cancel;

            return node.Next;
        }
    }
}
