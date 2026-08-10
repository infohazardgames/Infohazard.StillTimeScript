using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;
using Infohazard.StillTimeScript.Core.Parsers.Macros;
using Infohazard.StillTimeScript.Core.Utility;

namespace Infohazard.StillTimeScript.Core.Parsers {
    public static class CommandParserDelegator {
        private static readonly Dictionary<string, ICommandParser> AllCommandParsers = new();

        static CommandParserDelegator() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    CustomCommandParserAttribute[] attributes =
                        type.GetCustomAttributes<CustomCommandParserAttribute>().ToArray();

                    if (attributes.Length == 0) continue;

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
                        continue;
                    }

                    foreach (CustomCommandParserAttribute attribute in attributes) {
                        AllCommandParsers[attribute.CommandName] = commandParser;
                    }
                }
            }
        }

        public static void ParseLine(ParsingState state, List<ICommand> commands) {
            string line = state.CurrentLine;

            while (!state.IsEnded) {
                ReadOnlySpan<char> actualSpan = Tokenizer.GetActualSpanFromLine(line);

                if (!actualSpan.IsEmpty) {
                    break;
                } else {
                    state.MoveNext();
                    line = state.CurrentLine;
                }
            }

            if (state.IsEnded) return;

            string cmd = Tokenizer.TokenizeCommandName(state).ToString();

            int version = state.Version;

            if (AllCommandParsers.TryGetValue(cmd, out ICommandParser parser)) {
                parser.ParseCommand(state, commands);
            } else if (state.Macros.TryGetValue(cmd, out Macro macro)) {
                macro.ExpandCall(state);
            } else {
                throw new ParsingException(state.LineNumber, line, $"No parser or macro found for command '{cmd}'");
            }

            if (state.Version == version) {
                throw new ParsingException(state.LineNumber, line,
                                           $"Command '{cmd}' did not advance the parsing state");
            }
        }
    }
}
