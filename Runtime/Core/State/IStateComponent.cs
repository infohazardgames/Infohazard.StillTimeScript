using Infohazard.StillTimeScript.Core.Nodes;
using Newtonsoft.Json.Linq;

namespace Infohazard.StillTimeScript.Core.State {
    public interface IStateComponent {
        public IStateComponent Clone();

        public void Initialize(GameGraph graph) { }

        public JToken Serialize();

        public bool Deserialize(GameGraph graph, JToken token);
    }
}
