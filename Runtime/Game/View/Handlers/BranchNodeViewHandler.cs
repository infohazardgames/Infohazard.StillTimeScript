using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.Runner;
using Infohazard.StillTimeScript.Game.Utility;
using Infohazard.StillTimeScript.Game.View.Components;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public class BranchNodeViewHandler : NodeViewHandler<BranchNode> {
        public TextPromptView _view;
        public GameSettings _gameSettings;
        public StateAdvancer _stateAdvancer;
        public StateExplorer _stateExplorer;

        protected override async UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            BranchNode node,
            CancellationToken cancellationToken) {

            UniTaskCompletionSource<INode> tcs = new();

            Action cancel = () => tcs.TrySetCanceled();
            cancellationToken.Register(cancel);

            _view.SetChoices(
                GameUtility.DoStringInterpolation(node.Text, graph, state),
                node.Speaker,
                node.Options
                    .Where(o => o.IsAvailable(state))
                    .Select(o => {
                        INode next = o.GetNextNode(state);
                        string text = o.GetText(state);
                        List<StateContainer> stack = new() { state };
                        StateContainer testState = _stateAdvancer.AdvanceState(graph, state, next);
                        bool hasNewContent = _stateExplorer.ExploreBranchForNewContent(graph, stack, testState, 10_000);
                        return (text, new Action(() => tcs.TrySetResult(next)), hasNewContent);
                    })
                    .ToList(),
                _gameSettings.SkipAnimations);

            _view.Cancellation += cancel;

            INode next = await tcs.Task;
            _view.Cancellation -= cancel;

            return next;
        }
    }
}
