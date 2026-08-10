using System;
using Infohazard.StillTimeScript.Core.Commands.Interfaces;

namespace Infohazard.StillTimeScript.Core.Utility {
    public ref struct CommandGatheringState {
        private readonly ReadOnlySpan<ICommand> _commands;

        public readonly bool IsEnded => _commandIndex >= _commands.Length;

        private int _commandIndex;

        public readonly ICommand Current => _commandIndex < _commands.Length
            ? _commands[_commandIndex]
            : null;

        public CommandGatheringState(ReadOnlySpan<ICommand> commands, int index) {
            _commands = commands;
            _commandIndex = index;
        }

        public ICommand Take() {
            if (IsEnded) {
                throw new InvalidOperationException("Cannot take command from ended state.");
            }

            return _commands[_commandIndex++];
        }

        public T Take<T>() where T : ICommand {
            if (IsEnded) {
                throw new InvalidOperationException("Cannot take command from ended state.");
            }

            if (_commands[_commandIndex] is not T command) {
                throw new InvalidOperationException(
                    $"Expected command of type {typeof(T).Name}, but got {_commands[_commandIndex].GetType().Name}.");
            }

            _commandIndex++;
            return command;
        }

        public bool TryTake<T>(out T command) where T : ICommand {
            if (IsEnded) {
                command = default;
                return false;
            }

            if (_commands[_commandIndex] is not T typedCommand) {
                command = default;
                return false;
            }

            _commandIndex++;
            command = typedCommand;
            return true;
        }
    }
}
