namespace Infohazard.StillTimeScript.Core.Nodes {
    public class DelayNode : SequentialNode {
        public float Time { get; }

        public DelayNode(float time) {
            Time = time;
        }
    }
}
