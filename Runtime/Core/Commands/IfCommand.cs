using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    public class IfCommand : Command, ISequentialCommand {
        public Token Condition { get; }

        public List<ISequentialCommand> Commands { get; } = new();

        public List<ElseIfCommand> ElseIfCommands { get; } = new();

        public ElseCommand ElseCommand { get; set; }

        public IfCommand(int lineNumber, string line, Token condition) : base(lineNumber, line) {
            Condition = condition;
        }

        public override void GatherSubCommands(ref CommandGatheringState state) {
            CommandUtility.GatherSubCommands(this, ref state, Commands, false, true);
            CommandUtility.GatherSubCommands(this, ref state, ElseIfCommands, false, true);
            ElseCommand = CommandUtility.GatherSubCommand<ElseCommand>(ref state);
        }

        public void ApplyToSequence(NodeSequenceBuilder builder, GraphData graphData) {
            IExpression expression = ExpressionParser.ParseExpression(this, graphData, Line, Condition.Range);

            IfNode firstIfNode = new(expression);
            builder.Append(firstIfNode);

            EmptyNode convergenceNode = new();
            firstIfNode.TrueBranch = CreateBranch(builder, Commands, convergenceNode, graphData);

            IfNode lastIfNode = firstIfNode;
            foreach (ElseIfCommand elseIf in ElseIfCommands) {
                IExpression elseIfCondition =
                    ExpressionParser.ParseExpression(this, graphData, elseIf.Line, elseIf.Condition.Range);
                IfNode nextIfNode = new(elseIfCondition);
                builder.Append(nextIfNode);

                nextIfNode.TrueBranch = CreateBranch(builder, elseIf.Commands, convergenceNode, graphData);
                lastIfNode.FalseBranch = nextIfNode;
                lastIfNode = nextIfNode;
            }

            if (ElseCommand != null) {
                lastIfNode.FalseBranch = CreateBranch(builder, ElseCommand.Commands, convergenceNode, graphData);
            } else {
                lastIfNode.FalseBranch = convergenceNode;
            }

            builder.Append(convergenceNode);
        }

        private static INode CreateBranch(
            NodeSequenceBuilder builder,
            List<ISequentialCommand> branchCommands,
            EmptyNode convergenceNode,
            GraphData graphData) {

            if (branchCommands.Count > 0) {
                EmptyNode branchNode = new();
                builder.Append(branchNode);

                foreach (ISequentialCommand command in branchCommands) {
                    command.ApplyToSequence(builder, graphData);
                }

                builder.EndWithExternalNode(convergenceNode);
                return branchNode;
            } else {
                return convergenceNode;
            }
        }
    }
}
