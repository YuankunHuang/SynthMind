using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.CommandCore
{
    public class CommandManager : ICommandManager
    {
        private Dictionary<string, IGameCommand> _commands = new();

        public CommandManager()
        {
            RegisterCommand(new SpawnCommand());
            RegisterCommand(new BuildCommand());
            RegisterCommand(new MoveCommand());
            RegisterCommand(new ClearCommand());
        }

        public void RegisterCommand(IGameCommand command)
        {
            _commands[command.CommandName.ToLower()] = command;
            LogHelper.Log($"[CommandManager]::RegisterCommand: {command.CommandName}");
        }

        public void UnregisterCommand(string commandName)
        {
            _commands.Remove(commandName.ToLower());
        }

        public bool TryExecuteCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var parts = input.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1)
            {
                return false;
            }

            var commandName = parts[0].ToLower();
            var parameters = parts.Skip(0).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                if (command.CanExecute(parameters))
                {
                    LogHelper.Log($"[CommandManager]: Executing command: {commandName} with params: [{string.Join(", ", parameters)}]");
                    command.Execute(parameters);
                    return true;
                }
                else
                {
                    LogHelper.LogError($"Command {commandName} cannot execute with given parameters.");
                    return false;
                }
            }

            LogHelper.LogWarning($"Unknown command: {commandName}");
            return false;
        }

        public bool TryExecuteNatural(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            LogHelper.Log($"[CommandManager]::TryExecuteNatural: Processing '{input}'");

            var parseResult = NaturalLanguageProcessor.ParseNaturalInput(input);
            if (!parseResult.Success)
            {
                LogHelper.LogError($"[CommandManager]::TryExecuteNatural: Could not interpret '{input}'");
                return false;
            }

            LogHelper.Log($"[CommandManager]::TryExecuteNatural: Interpreted: {parseResult.OriginalInput} -> '{parseResult.Command}'");

            return TryExecuteCommand(parseResult.Command);
        }

        public string[] GetAvailableCommands()
        {
            return _commands.Keys.ToArray();
        }

        public void Dispose()
        {
            _commands.Clear();
        }
    }
}