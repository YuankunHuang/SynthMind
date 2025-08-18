using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.CommandCore
{
    public class ClearCommand : IGameCommand
    {
        public string CommandName => "clear";
        public string Description => "Clears sandbox. Usage: clear all";

        public bool CanExecute(string[] parameters)
        {
            return parameters.Length >= 1 && parameters[1].ToLower() == "all" &&
                   SandboxManager.Instance != null;
        }

        public void Execute(string[] parameters)
        {
            SandboxManager.Instance.Clear();
        }
    }
}