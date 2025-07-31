using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class ClearCommand : IGameCommand
    {
        public string CommandName => "clear";
        public string Description => "Clears objects from scene. Usage: clear all";

        public bool CanExecute(string[] parameters)
        {
            return parameters != null && parameters.Length > 0 && parameters[0].ToLower() == "all";
        }

        public void Execute(string[] parameters)
        {
            var objs = GameObject.FindGameObjectsWithTag("AI_Spawned");
            foreach (var obj in objs)
            {
                GameObject.Destroy(obj);
            }

            LogHelper.Log($"Cleared all AI-spawned objects.");
        }
    }
}