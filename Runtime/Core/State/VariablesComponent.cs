using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.StillTimeScript.Core.Nodes;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.Utility;
using Newtonsoft.Json.Linq;

namespace Infohazard.StillTimeScript.Core.State {
    public class VariablesComponent : IScopedComponent {
        public Dictionary<Variable, StsValue> Variables { get; }

        public VariablesComponent() {
            Variables = new Dictionary<Variable, StsValue>();
        }

        public VariablesComponent(Dictionary<Variable, StsValue> variables) {
            Variables = variables;
        }

        public void SetVariableValue(Variable variable, StsValue value) {
            if (variable.DefaultValue.GetType() != value.GetType()) {
                throw new ArgumentException($"Invalid value {value} specified for variable {variable}.");
            }

            Variables[variable] = value;
        }

        public StsValue GetVariableValue(Variable variable) {
            return Variables.TryGetValue(variable, out StsValue value) ? value : variable.DefaultValue;
        }

        public void ResetScope(Scope scope) {
            foreach (Variable variable in Variables.Keys.ToArray()) {
                if (variable.ScopeId != scope.Identifier) continue;
                Variables.Remove(variable);
            }
        }

        public IStateComponent Clone() {
            return new VariablesComponent(new Dictionary<Variable, StsValue>(Variables));
        }

        public JToken Serialize() {
            Dictionary<string, string> data = new();
            foreach ((Variable variable, StsValue value) in Variables) {
                data[variable.Identifier] = value.ToString();
            }

            return JToken.FromObject(data);
        }

        public bool Deserialize(GameGraph graph, JToken token) {
            Dictionary<string, string> data = token.ToObject<Dictionary<string, string>>();

            foreach ((string key, string strValue) in data) {
                if (!graph.TryGetResource(key, out Variable variable) ||
                    !StsValue.TryParse(strValue, variable.Type, out StsValue value)) continue;

                Variables[variable] = value;
            }

            return true;
        }
    }
}
