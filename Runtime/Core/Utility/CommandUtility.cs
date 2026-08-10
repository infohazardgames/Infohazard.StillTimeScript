using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;

namespace Infohazard.StillTimeScript.Core.Utility {
    public static class CommandUtility {
        public static void AssignIds(string identifierBase, NodeSequenceBuilder builder, GraphData graphData) {
            Dictionary<string, int> countByLocalId = new();
            foreach (INode createdNode in builder.NodeList) {
                createdNode.RegisterStateTypes(graphData.RequiredStates);
                string localId = createdNode.GetSelfIdentifier();
                countByLocalId.TryGetValue(localId, out int count);
                string globalId = $"{identifierBase}{localId}{count}";
                createdNode.FullIdentifier = globalId;
                countByLocalId[localId] = count + 1;
                graphData.Nodes.Add(globalId, createdNode);
            }
        }

        public static void GatherSubCommands<T>(
            ICommand parent,
            ref CommandGatheringState state,
            List<T> results,
            bool errorOnMismatch = true,
            bool stopBeforeEnd = false)
            where T : ICommand {
            bool isFirst = true;
            bool isSingleLine = false;
            while (!state.IsEnded) {
                if (isFirst) isSingleLine = state.Current.LineNumber == parent.LineNumber;
                isFirst = false;

                if (isSingleLine && state.Current.LineNumber > parent.LineNumber) break;
                if (stopBeforeEnd && state.Current is EndCommand) break;
                if (!errorOnMismatch && state.Current is not T) break;

                ICommand command = state.Take();
                if (command is EndCommand) break;

                if (command is not T typedCommand) {
                    throw new ParsingException(
                        state.Current.LineNumber,
                        state.Current.Line,
                        $"Expected command of type {typeof(T).Name} but got {state.Current.GetType().Name}");
                }

                results.Add(typedCommand);
                typedCommand.GatherSubCommands(ref state);
            }
        }

        public static T GatherSubCommand<T>(
            ref CommandGatheringState state,
            bool errorOnMismatch = true,
            bool stopBeforeEnd = false)
            where T : class, ICommand {
            if (state.IsEnded) return null;
            if (stopBeforeEnd && state.Current is EndCommand) return null;
            if (!errorOnMismatch && state.Current is not T) return null;

            ICommand command = state.Take();
            if (command is EndCommand) return null;

            if (command is not T typedCommand) {
                throw new ParsingException(
                    state.Current.LineNumber,
                    state.Current.Line,
                    $"Expected command of type {typeof(T).Name} but got {state.Current.GetType().Name}");
            }

            typedCommand.GatherSubCommands(ref state);
            return typedCommand;
        }
    }
}