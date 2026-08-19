using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Parsers.Macros;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    public static class CommandParserDelegator {
        private static readonly Dictionary<string, ICommandParser> CustomParsers = new();
        private static readonly Dictionary<string, (Type, AutoCommandParserAttribute)> AutoParsedTypes = new();

        static CommandParserDelegator() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    CheckCustomParserAttributes(type);
                    CheckAutoParserAttributes(type);
                }
            }
        }

        public static bool IsCommand(string commandName) {
            return CustomParsers.ContainsKey(commandName) || AutoParsedTypes.ContainsKey(commandName);
        }

        private static void CheckCustomParserAttributes(Type type) {
            CustomCommandParserAttribute[] attributes =
                type.GetCustomAttributes<CustomCommandParserAttribute>().ToArray();

            if (attributes.Length == 0) return;

            ICommandParser commandParser;
            try {
                object instance = Activator.CreateInstance(type);
                if (instance is not ICommandParser tempParser) {
                    throw new Exception($"Type {type} is not assignable to ICommandParser");
                }

                commandParser = tempParser;
            } catch (Exception ex) {
                StsLibrary.LogException(
                    new Exception($"Exception occurred instantiated command parser {type}", ex));
                return;
            }

            foreach (CustomCommandParserAttribute attribute in attributes) {
                CustomParsers[attribute.CommandName] = commandParser;
            }
        }

        private static void CheckAutoParserAttributes(Type type) {
            AutoCommandParserAttribute[] attributes =
                type.GetCustomAttributes<AutoCommandParserAttribute>().ToArray();

            if (attributes.Length == 0) return;

            if (!typeof(ICommand).IsAssignableFrom(type)) {
                throw new Exception($"Type {type} is not assignable to ICommand");
            }

            foreach (AutoCommandParserAttribute attribute in attributes) {
                AutoParsedTypes[attribute.CommandName] = (type, attribute);
            }
        }

        public static void ParseLine(ParsingState state, List<ICommand> commands) {
            ParsingState.LineInfo line = state.CurrentLine;

            StsRange actualRange;
            while (true) {
                actualRange = Tokenizer.GetActualRangeFromLine(line.Line, line.RangeInLine, out _);

                if (actualRange.Length > 0) {
                    break;
                } else {
                    state.MoveNext();
                    if (state.IsEnded) return;
                    line = state.CurrentLine;
                }
            }

            string cmd = Tokenizer.TokenizeCommandName(state.LineNumber, line.Line, actualRange).ToString();

            int version = state.Version;

            if (CustomParsers.TryGetValue(cmd, out ICommandParser parser)) {
                parser.ParseCommand(state, commands);
            } else if (AutoParsedTypes.TryGetValue(cmd, out (Type, AutoCommandParserAttribute) item)) {
                AutoCommandParserAttribute attr = item.Item2;
                LineTokens tokens = Tokenizer.TokenizeAndAdvance(state);
                Tokenizer.ValidateTokens(tokens, attr.MinArgCount, attr.MaxArgCount, attr.RequireText,
                                         attr.OptionalText);

                try {
                    ICommand command = (ICommand) Activator.CreateInstance(item.Item1, tokens);
                    commands.Add(command);
                } catch (Exception ex) {
                    throw new ParsingException(tokens.LineNumber, tokens.OriginalLine,
                                               $"Error occurred when constructing auto-parsed command:\n{ex}");
                }
            } else if (state.Macros.TryGetValue(cmd, out Macro macro)) {
                macro.ExpandCall(state);
            } else {
                throw new ParsingException(state.LineNumber, line.Line, $"No parser or macro found for command '{cmd}'");
            }

            if (state.Version == version) {
                throw new ParsingException(state.LineNumber, line.Line,
                                           $"Command '{cmd}' did not advance the parsing state");
            }
        }
    }
}
