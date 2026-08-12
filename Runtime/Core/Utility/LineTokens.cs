namespace Infohazard.StillTimeScript.Core.Utility {
    public struct LineTokens {
        public int LineNumber { get; }
        public string OriginalLine { get; }
        public string Command { get; }
        public string[] Arguments { get; }
        public string Text { get; }

        public LineTokens(
            int lineNumber,
            string originalLine,
            string command,
            string[] arguments,
            string text) {
            LineNumber = lineNumber;
            OriginalLine = originalLine;
            Command = command;
            Arguments = arguments;
            Text = text;
        }

        public string GetArg(int index) {
            return Arguments != null && Arguments.Length > index ? Arguments[index] : null;
        }
    }
}
