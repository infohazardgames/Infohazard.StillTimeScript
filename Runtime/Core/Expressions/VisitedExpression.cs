using System;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class VisitedExpression : IExpression {
        public StsValueType Type => StsValueType.Bool;
        
        public IExpression ScopeExpression { get; }
        
        public IExpression NodeExpression { get; }
        
        public VisitedExpression(IExpression scopeExpression, IExpression nodeExpression) {
            if (scopeExpression.Type != StsValueType.Resource ||
                nodeExpression.Type != StsValueType.Node) {
                throw new Exception("Expected resource and node expressions for 'visited' function");
            }
            
            ScopeExpression = scopeExpression;
            NodeExpression = nodeExpression;
        }
        
        public StsValue Evaluate(StateContainer state) {
            Scope scope = ScopeExpression.Evaluate(state).ResourceValue as Scope;
            INode node = NodeExpression.Evaluate(state).NodeValue;

            if (scope == null) {
                StsLibrary.LogError("Scope expression did not evaluate to a scope");
                return new StsValue(false);
            } else if (node == null) {
                StsLibrary.LogError("Node expression did not evaluate to a node");
                return new StsValue(false);
            }
            
            bool visited = state.GetOrCreate<VisitedNodesComponent>().IsVisited(scope, node);
            return new StsValue(visited);
        }
    }
}