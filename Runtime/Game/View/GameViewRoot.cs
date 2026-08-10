using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.View.Components;
using Infohazard.StillTimeScript.Game.View.Handlers;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.View {
    public class GameViewRoot : MonoBehaviour {
        public List<NodeViewHandler> _nodeViewHandlers;
        public List<AnyNodeViewHandler> _anyNodeViewHandlers;
        public List<GameViewComponent> _gameViewComponents;

        private readonly Dictionary<Type, NodeViewHandler> _nodeHandlersByType = new();

        private void OnEnable() {
            _nodeHandlersByType.Clear();

            foreach (NodeViewHandler handler in _nodeViewHandlers) {
                foreach (Type nodeType in handler.HandledNodeTypes) {
                    _nodeHandlersByType.TryAdd(nodeType, handler);
                }
            }
        }

        public void HandleInitialState(GameGraph gameGraph, StateContainer currentState) {
            foreach (AnyNodeViewHandler handler in _anyNodeViewHandlers) {
                handler.HandleInitialState(gameGraph, currentState);
            }

            foreach (NodeViewHandler handler in _nodeViewHandlers) {
                handler.HandleInitialState(gameGraph, currentState);
            }
        }

        public void UpdateViewFromState(GameGraph gameGraph, StateContainer currentState) {
            foreach (AnyNodeViewHandler anyHandler in _anyNodeViewHandlers) {
                anyHandler.HandleState(gameGraph, currentState);
            }
        }

        public bool TryHandleStateInView(
            GameGraph gameGraph,
            StateContainer currentState,
            out UniTask<INode> task,
            CancellationToken cancellationToken) {

            INode currentNode = currentState.GetOrCreate<CurrentNodeComponent>().CurrentNode;
            Type nodeType = currentNode.GetType();
            if (_nodeHandlersByType.TryGetValue(nodeType, out NodeViewHandler handler)) {
                task = handler.HandleState(gameGraph, currentState, cancellationToken);
                return true;
            } else {
                task = default;
                return false;
            }
        }

        public void Clear() {
            foreach (GameViewComponent component in _gameViewComponents) {
                component.Clear();
            }
        }
    }
}
