using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Expressions;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Parsers;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Commands {
    [AutoCommandParser("choice", 1, 2, true)]
    public class ChoiceCommand : TextCommand, IBranchSubCommand {
        public Token TargetStr { get; }
        public Token? ConditionStr { get; }

        public ChoiceCommand(LineTokens tokens) : base(tokens, null, tokens.GetRequiredText()) {
            TargetStr = tokens.GetRequiredArg(0);
            ConditionStr = tokens.GetArg(1);
        }

        public void CreateBranchOptions(GraphData graphData, List<IBranchOption> options) {
            IExpression targetExpr =
                ExpressionParser.ParseExpression(this, graphData, Line, TargetStr.Range, StsValueType.Node);
            IExpression condExpr = ConditionStr == null
                ? null
                : ExpressionParser.ParseExpression(this, graphData, Line, ConditionStr.Value.Range);

            Choice choice = new(GetTextExpression(graphData), targetExpr, condExpr);

            options.Add(choice);
        }

        public override IEnumerable<CommandToken> EnumerateTokens() {
            foreach (CommandToken token in base.EnumerateTokens()) {
                yield return token;
            }

            yield return new CommandToken(TargetStr, CommandTokenType.Expression, StsValueType.Node);

            if (ConditionStr != null) {
                yield return new CommandToken(ConditionStr.Value, CommandTokenType.Expression);
            }
        }
    }
}
