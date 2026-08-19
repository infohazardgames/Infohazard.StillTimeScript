using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    public struct LineTokens {
        public int LineNumber { get; }
        public string OriginalLine { get; }
        public Token Command { get; }
        public Token[] Arguments { get; }
        public Token? Text { get; }

        public LineTokens(
            int lineNumber,
            string originalLine,
            Token command,
            Token[] arguments,
            Token? text) {
            LineNumber = lineNumber;
            OriginalLine = originalLine;
            Command = command;
            Arguments = arguments;
            Text = text;
        }

        public Token? GetArg(int index) {
            return Arguments != null && Arguments.Length > index ? Arguments[index] : null;
        }

        public Token GetRequiredArg(int index) {
            return Arguments != null && Arguments.Length > index
                ? Arguments[index]
                : throw new ArgumentException($"Argument at index {index} is required but not provided.");
        }

        public Token GetRequiredText() {
            return Text ?? throw new ArgumentException("Text is required but not provided.");
        }
    }
}
