using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Game.View;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Infohazard.StillTimeScript.Game.Runner {
    public class GameRunner : MonoBehaviour {
        public GameViewRoot _gameViewRoot;
        public StateAdvancer _stateAdvancer;

        private GameGraph _gameGraph;
        private StateContainer _currentState;
        private CancellationTokenSource _cancellationTokenSource;

        public void LoadGameGraph(GameGraph gameGraph) {
            _gameGraph = gameGraph;
        }

        public void StartNewGame() {
            if (_gameGraph == null) {
                throw new InvalidOperationException("Game graph must be loaded before starting game.");
            }

            if (_currentState != null) {
                throw new InvalidOperationException("Game is already running.");
            }

            StateContainer state = _gameGraph.BuildEmptyState();
            state.GetOrCreate<CurrentNodeComponent>().CurrentNode = _gameGraph.RootNode;
            _cancellationTokenSource = new CancellationTokenSource();
            Run(state, _cancellationTokenSource.Token).Forget();
        }

        public void LoadGame(JToken data) {
            if (_gameGraph == null) {
                throw new InvalidOperationException("Game graph must be loaded before starting game.");
            }

            if (_currentState != null) {
                throw new InvalidOperationException("Game is already running.");
            }

            StateContainer state = _gameGraph.BuildEmptyState();
            if (!state.Deserialize(_gameGraph, data)) {
                throw new Exception("Failed to deserialize state");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Run(state, _cancellationTokenSource.Token).Forget();
        }

        public JToken SaveGame() {
            if (_gameGraph == null) {
                throw new InvalidOperationException("Game graph must be loaded before starting game.");
            }

            if (_currentState == null) {
                throw new InvalidOperationException("Game is not running.");
            }

            return _currentState.Serialize();
        }

        public void ClearGameState() {
            if (_gameGraph == null) {
                throw new InvalidOperationException("Game graph must be loaded before starting game.");
            }

            _gameViewRoot.Clear();
            _currentState = null;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }


        private async UniTask Run(StateContainer initialState, CancellationToken cancellationToken) {
            HashSet<INode> synchronousSeenNodes = new();
            _currentState = initialState;

            _gameViewRoot.HandleInitialState(_gameGraph, _currentState);

            while (_currentState.GetOrCreate<CurrentNodeComponent>() is { CurrentNode: { } currentNode }) {
                if (!synchronousSeenNodes.Add(currentNode)) {
                    throw new Exception(
                        $"Encountered a node {currentNode.FullIdentifier} twice in the same synchronous call to Run. " +
                        "This could easily result in an infinite loop.");
                }

                if (!isActiveAndEnabled) return;

                _gameViewRoot.UpdateViewFromState(_gameGraph, _currentState);

                Type nodeType = currentNode.GetType();

                INode nextNode;
                if (_gameViewRoot.TryHandleStateInView(
                        _gameGraph, _currentState, out UniTask<INode> task, cancellationToken)) {

                    if (task.Status == UniTaskStatus.Pending) {
                        synchronousSeenNodes.Clear();
                    }

                    nextNode = await task;
                } else if (currentNode is ISingleNextNode singleNextNode) {
                    nextNode = singleNextNode.GetSingleNextNode(_currentState);
                } else {
                    Debug.LogError($"Encountered a node type that could not be traversed: {nodeType}");
                    return;
                }

                _currentState = _stateAdvancer.AdvanceState(_gameGraph, _currentState, nextNode);
                Debug.Log(
                    $"Going to next state '{_currentState.GetOrCreate<CurrentNodeComponent>().CurrentNode?.FullIdentifier}'");
            }

            _gameViewRoot.Clear();
        }
    }
}
