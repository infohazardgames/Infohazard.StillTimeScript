using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    public class ParsingException : Exception {
        public int LineNumber { get; set; }
        public string Text { get; set; }
        public string CustomMessage { get; set; }
        
        public ParsingException(int lineNumber, string text, string customMessage = null) : 
            base($"line {lineNumber + 1}: {customMessage ?? "Parsing error"}\nFull line: '{text?.Trim()}'") {

            LineNumber = lineNumber;
            Text = text;
            CustomMessage = customMessage;
        }
    }
}