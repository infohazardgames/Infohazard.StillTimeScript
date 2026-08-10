using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Utility {
    public static class GraphBuilder {
        public static GameGraph BuildGraph(List<ICommand> commands) {
            GraphData graphData = GraphData.Empty();
            graphData.RequiredStates.Add(typeof(VariablesComponent));
            graphData.RequiredStates.Add(typeof(CurrentNodeComponent));
            graphData.RequiredStates.Add(typeof(VisitedNodesComponent));

            foreach (IResourceCommand resourceCommand in commands.OfType<IResourceCommand>()) {
                resourceCommand.CreateResources(graphData);
            }

            foreach (IResourceCommand resourceCommand in commands.OfType<IResourceCommand>()) {
                resourceCommand.ValidateResources(graphData);
            }

            CommandGatheringState state = new(commands.ToArray(), 0);

            List<ICommand> unFlattenedCommands = new();
            while (!state.IsEnded) {
                ICommand command = state.Take();
                command.GatherSubCommands(ref state);
                unFlattenedCommands.Add(command);
            }

            foreach (ISubtreeCommand command in unFlattenedCommands.OfType<ISubtreeCommand>()) {
                command.BuildSubtree(graphData);
            }

            NodeSequenceBuilder builder = new();

            foreach (ISequentialCommand command in unFlattenedCommands.OfType<ISequentialCommand>()) {
                command.ApplyToSequence(builder, graphData);
            }

            CommandUtility.AssignIds(string.Empty, builder, graphData);

            return new GameGraph(builder.FirstNode, graphData.Nodes, graphData.Resources,
                                 graphData.RequiredStates.ToList());
        }

    }
}
