using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.SandboxCore
{
    public class SpawnCommand : IGameCommand
    {
        public string CommandName => "spawn";
        public string Description => "Spawns objects in sandbox. Usage: spawn [object] [x] [z]";

        public bool CanExecute(string[] parameters)
        {
            return parameters != null && parameters.Length > 0 && SandboxManager.Instance != null;
        }

        public void Execute(string[] parameters)
        {
            var objType = parameters[1];
            var pos = GetSpawnPosition(parameters.Skip(2).ToArray());
            switch (objType)
            {
                case "tree":
                    SpawnTree(pos);
                    break;
                case "house":
                    SpawnHouse(pos);
                    break;
                case "rock":
                    SpawnRock(pos);
                    break;
                default:
                    LogHelper.LogError($"Undefined obj: {objType}");
                    break;
            }
        }

        private void SpawnTree(Vector3 position)
        {
            // Create tree using SandboxManager
            var trunk = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cylinder,
                "AI_Tree_Trunk",
                position,
                new Vector3(0.3f, 1f, 0.3f),
                new Color(0.4f, 0.2f, 0.1f)
            );

            var foliage = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Sphere,
                "AI_Tree_Foliage",
                position + Vector3.up * 1.2f,
                new Vector3(1.5f, 1.2f, 1.5f),
                new Color(0.2f, 0.8f, 0.2f)
            );

            // Parent foliage to trunk
            foliage.transform.SetParent(trunk.transform);

            LogHelper.Log($"Spawned tree at {position}");
        }

        private void SpawnHouse(Vector3 position)
        {
            var house = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "AI_House",
                position,
                new Vector3(2f, 1.5f, 2f),
                new Color(0.8f, 0.6f, 0.4f)
            );

            LogHelper.Log($"Spawned house at {position}");
        }

        private void SpawnRock(Vector3 position)
        {
            var rock = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Sphere,
                "AI_Rock",
                position,
                new Vector3(0.8f, 0.6f, 0.8f),
                new Color(0.5f, 0.5f, 0.5f)
            );

            LogHelper.Log($"Spawned rock at {position}");
        }

        private Vector3 GetSpawnPosition(string[] positionParams)
        {
            if (positionParams.Length > 1)
            {
                if (float.TryParse(positionParams[0], out var x) &&
                    float.TryParse(positionParams[1], out var z))
                {
                    return new Vector3(x, 0, z);
                }
            }

            if (positionParams.Length > 0)
            {
                switch (positionParams[0].ToLower())
                {
                    case "left":
                        return new Vector3(-3, 0, 0);
                    case "right":
                        return new Vector3(3, 0, 0);
                    case "center":
                        return new Vector3(0, 0, 0);
                    case "front":
                        return new Vector3(0, 0, 3);
                    case "back":
                        return new Vector3(0, 0, -3);
                }
            }

            return new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
        }
    }
}