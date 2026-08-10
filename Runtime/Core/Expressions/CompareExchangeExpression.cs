#nullable enable

using System;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Expressions {
    public class CompareExchangeExpression : IExpression {
        public StsValueType Type => Variable.Type;
        
        public Variable Variable { get; }
        
        public IExpression? Comparand { get; }
        
        public IExpression? ReplacementValue { get; }
        
        public CompareExchangeExpression(
            Variable variable, 
            IExpression? comparand = null,
            IExpression? replacementValue = null) {
            
            if (comparand != null && comparand.Type != variable.Type) {
                throw new ArgumentException("Comparand type does not match variable type");
            }
            
            if (replacementValue != null && replacementValue.Type != variable.Type) {
                throw new ArgumentException("Replacement value type does not match variable type");
            }
            
            Variable = variable;
            Comparand = comparand;
            ReplacementValue = replacementValue;
        }
        
        public StsValue Evaluate(StateContainer state) {
            VariablesComponent variables = state.GetOrCreate<VariablesComponent>();
            StsValue variableValue = variables.GetVariableValue(Variable);

            bool passes;
            if (Comparand != null) {
                StsValue comparandValue = Comparand.Evaluate(state);
                passes = comparandValue.Equals(variableValue);
            } else {
                passes = variableValue.ToBool();
            }
            
            if (!passes) return variableValue;

            StsValue storeValue;
            if (ReplacementValue != null) {
                storeValue = ReplacementValue.Evaluate(state);
            } else {
                storeValue = StsValue.Default(Variable.Type);
            }
            
            variables.SetVariableValue(Variable, storeValue);

            return variableValue;
        }
    }
}