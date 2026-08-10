using System.Collections.Generic;
using Infohazard.StillTimeScript.Core.Resource;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public class SayNode : TextNode, ISequentialNode {
        public INode Next { get; set; }

        public SayNode(string text, Speaker speaker) : base(text, speaker) { }

        public override IEnumerable<INode> GetPossibleNextNodes(StateContainer state) {
            yield return Next;
        }

        public INode GetSingleNextNode(StateContainer state) {
            return Next;
        }
    }
}
