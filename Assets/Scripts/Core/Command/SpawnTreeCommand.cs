using System.Linq;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class SpawnTreeCommand : IGameCommand
    {
        public string CommandName => "spawn";
        public string Description => "Spawns objects in the scene. Usage: spawn tree [position]";

        public bool CanExecute(string[] parameters)
        {
            return parameters.Length >= 1 && parameters[0].ToLower() == "tree";
        }

        public void Execute(string[] parameters)
        {
            Vector3 position = GetSpawnPosition(parameters.Skip(1).ToArray());

            // Create a simple tree (cube for now, replace with actual prefab)
            var tree = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tree.name = "AI_Tree";
            tree.transform.position = position;
            tree.transform.localScale = new Vector3(0.5f, 2f, 0.5f);

            // Add a simple material
            var renderer = tree.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.8f, 0.2f); // Green

            LogHelper.Log($"Spawned tree at position {position}");
        }

        private Vector3 GetSpawnPosition(string[] positionParams)
        {
            if (positionParams.Length >= 2)
            {
                if (float.TryParse(positionParams[0], out float x) &&
                    float.TryParse(positionParams[1], out float z))
                {
                    return new Vector3(x, 0, z);
                }
            }

            if (positionParams.Length >= 1)
            {
                switch (positionParams[0].ToLower())
                {
                    case "left": return new Vector3(-3, 0, 0);
                    case "right": return new Vector3(3, 0, 0);
                    case "center": return Vector3.zero;
                }
            }

            // Random position
            return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        }
    }
}
