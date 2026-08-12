using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomCommandParserAttribute : Attribute {
        public string CommandName { get; }

        public CustomCommandParserAttribute(string commandName) {
            CommandName = commandName;
        }
    }
}
