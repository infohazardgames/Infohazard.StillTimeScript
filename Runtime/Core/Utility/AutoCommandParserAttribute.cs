using System;

namespace Infohazard.StillTimeScript.Core.Utility {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AutoCommandParserAttribute : Attribute {
        public string CommandName { get; }

        public int MinArgCount { get; }

        public int MaxArgCount { get; }

        public bool RequireText { get; }

        public bool OptionalText { get; }

        public AutoCommandParserAttribute(
            string commandName,
            int minArgCount = 0,
            int maxArgCount = 0,
            bool requireText = false,
            bool optionalText = false) {
            CommandName = commandName;
            MinArgCount = minArgCount;
            MaxArgCount = maxArgCount >= minArgCount ? maxArgCount : minArgCount;
            RequireText = requireText;
            OptionalText = optionalText;
        }
    }
}
