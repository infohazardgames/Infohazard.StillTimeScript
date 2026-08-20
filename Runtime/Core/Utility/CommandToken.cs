namespace Infohazard.StillTimeScript.Core.Utility {
    public struct CommandToken {
        public Token Token { get; }
        public CommandTokenType Type { get; }
        public StsValueType RequiredValueType { get; }

        public CommandToken(Token token, CommandTokenType type, StsValueType requiredValueType = StsValueType.None) {
            Token = token;
            Type = type;
            RequiredValueType = requiredValueType;
        }
    }

    public enum CommandTokenType {
        Expression,
        Definition,
        Keyword,
        MacroCall,
        StringExpression,
        ResourceReference,
        NodeReference,
        ColorLiteral,
        StringLiteral,
    }
}
