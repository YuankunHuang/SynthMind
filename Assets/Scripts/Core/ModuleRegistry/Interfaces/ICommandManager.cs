using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.SandboxCore
{
    public interface ICommandManager : IModule
    {
        void RegisterCommand(IGameCommand command);
        void UnregisterCommand(string commandName);
        bool TryExecuteCommand(string input);
        bool TryExecuteNatural(string input);
        string[] GetAvailableCommands();
    }

    public interface IGameCommand
    {
        string CommandName { get; }
        string Description { get; }
        bool CanExecute(string[] parameters);
        void Execute(string[] parameters);
    }
}