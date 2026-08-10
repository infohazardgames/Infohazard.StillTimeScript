using System;
using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Nodes;
using Newtonsoft.Json.Linq;

namespace Infohazard.StillTimeScript.Core.State {
    public class StateContainer {
        private readonly Dictionary<Type, IStateComponent> _components;

        public IReadOnlyDictionary<Type, IStateComponent> Components => _components;

        public StateContainer() {
            _components = new Dictionary<Type, IStateComponent>();
        }

        public StateContainer(Dictionary<Type, IStateComponent> components) {
            _components = components;
        }

        public bool TryGet<T>(out T component) where T : IStateComponent {
            bool hasComponent = _components.TryGetValue(typeof(T), out IStateComponent value);
            component = hasComponent ? (T) value : default;
            return hasComponent;
        }

        public void Set(Type type, IStateComponent component) {
            _components[type] = component;
        }

        public T GetOrCreate<T>() where T : IStateComponent, new() {
            if (TryGet(out T component)) return component;
            component = new T();
            _components[typeof(T)] = component;
            return component;
        }

        public StateContainer Clone() {
            Dictionary<Type, IStateComponent> cloneDictionary = new();
            foreach ((Type type, IStateComponent component) in _components) {
                IStateComponent newComponent = component.Clone();
                if (!type.IsInstanceOfType(newComponent)) {
                    throw new Exception($"Component {component} created clone of wrong type {newComponent}");
                }

                cloneDictionary[type] = newComponent;
            }

            return new StateContainer(cloneDictionary);
        }

        public JToken Serialize() {
            JObject result = new();
            foreach ((Type type, IStateComponent component) in _components) {
                result[type.Name] = component.Serialize();
            }

            return result;
        }

        public bool Deserialize(GameGraph graph, JToken token) {
            if (token is not JObject obj) return false;
            foreach ((Type type, IStateComponent component) in _components) {
                if (!obj.TryGetValue(type.Name, out JToken componentState)) {
                    componentState = JValue.CreateNull();
                }

                if (!component.Deserialize(graph, componentState)) return false;
            }

            return true;
        }
    }
}
