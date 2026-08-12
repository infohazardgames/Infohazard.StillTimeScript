using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;
using Infohazard.StillTimeScript.Game.Utility;
using Infohazard.StillTimeScript.Game.View.Components;

namespace Infohazard.StillTimeScript.Game.View.Handlers {
    public class BgNodeViewHandler : NodeViewHandler<BgNode> {
        public BackgroundView _view;
        public GameSettings _gameSettings;

        protected override UniTask<INode> HandleState(
            GameGraph graph,
            StateContainer state,
            BgNode node,
            CancellationToken cancellationToken) {
            StsColor color = node.Color.Evaluate(state).ColorValue;
            float time = (float) node.Time.Evaluate(state).NumberValue;

            _view.SetColor(color, _gameSettings.SkipAnimations ? 0 : time);
            return UniTask.FromResult(node.Next);
        }

        public override void HandleInitialState(GameGraph graph, StateContainer state) {
            if (!graph.TryGetResource(BgCommand.BuiltInVariableName, out Variable variable)) return;

            _view.SetColor(state.GetOrCreate<VariablesComponent>().GetVariableValue(variable).ColorValue, 0);
        }
    }
}
