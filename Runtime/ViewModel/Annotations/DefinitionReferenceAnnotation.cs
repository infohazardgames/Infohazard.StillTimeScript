using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.ViewModel.Annotations {
    public class DefinitionReferenceAnnotation : LineAnnotation {
        public Token? DefinitionToken { get; }
        public DefinitionReferenceAnnotation(StsRange range, Token? definitionToken) : base(range) {
            DefinitionToken = definitionToken;
        }
    }
}
