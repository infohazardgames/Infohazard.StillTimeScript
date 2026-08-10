using System;
using System.Collections.Generic;
using System.Diagnostics;
using Infohazard.StillTimeScript.Core.State;

namespace Infohazard.StillTimeScript.Core.Nodes {
    public abstract class Node : INode {
        public string FullIdentifier { get; set; }

        public StackTrace CreationStackTrace { get; }

        protected Node() {
#if UNITY_EDITOR
            CreationStackTrace = new StackTrace(2);
#endif
        }

        public virtual string GetSelfIdentifier() {
            return GetType().Name;
        }

        public abstract IEnumerable<INode> GetPossibleNextNodes(StateContainer state);

        public virtual void ApplyAfterAdvanceToSelf(GameGraph graph, StateContainer state) { }

        public virtual void ApplyBeforeAdvanceFromSelf(GameGraph graph, StateContainer state, ref INode nextNode) { }

        public virtual void RegisterStateTypes(HashSet<Type> stateTypes) { }
    }
}
