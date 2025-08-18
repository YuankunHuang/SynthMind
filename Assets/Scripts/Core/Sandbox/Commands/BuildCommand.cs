using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.CommandCore
{
    public class BuildCommand : IGameCommand
    {
        public string CommandName => "build";
        public string Description => "Builds structures in sandbox. Usage: build [structure] [x] [z]";

        public bool CanExecute(string[] parameters)
        {
            return parameters.Length >= 1 && SandboxManager.Instance != null;
        }

        public void Execute(string[] parameters)
        {
            var structureType = parameters[0].ToLower();
            Vector3 position = GetBuildPosition(parameters.Skip(1).ToArray());

            switch (structureType)
            {
                case "house":
                    BuildHouse(position);
                    break;
                case "tower":
                    BuildTower(position);
                    break;
                case "wall":
                    BuildWall(position);
                    break;
                case "bridge":
                    BuildBridge(position);
                    break;
                case "castle":
                    BuildCastle(position);
                    break;
                case "village":
                    BuildVillage(position);
                    break;
                default:
                    LogHelper.LogError($"Unknown structure type: {structureType}. Available: house, tower, wall, bridge, castle, village");
                    break;
            }
        }

        private void BuildHouse(Vector3 position)
        {
            var house = new GameObject("AI_House");
            house.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            house.transform.position = position;
            house.tag = TagNames.AISpawned;

            // House base
            var baseObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "House_Base",
                position,
                new Vector3(3f, 2f, 3f),
                new Color(0.8f, 0.6f, 0.4f)  // Brown
            );
            baseObj.transform.SetParent(house.transform);

            // House roof
            var roofObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "House_Roof",
                position + Vector3.up * 2.2f,
                new Vector3(3.5f, 0.4f, 3.5f),
                Color.red
            );
            roofObj.transform.SetParent(house.transform);

            // Door
            var doorObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "House_Door",
                position + Vector3.forward * 1.5f + Vector3.up * 0.8f,
                new Vector3(0.8f, 1.6f, 0.1f),
                new Color(0.4f, 0.2f, 0.1f)  // Dark brown
            );
            doorObj.transform.SetParent(house.transform);

            LogHelper.Log($"Built house at {position}");
        }

        private void BuildTower(Vector3 position)
        {
            var tower = new GameObject("AI_Tower");
            tower.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            tower.transform.position = position;
            tower.tag = TagNames.AISpawned;

            // Tower base (wider)
            var baseObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cylinder,
                "Tower_Base",
                position,
                new Vector3(2f, 1f, 2f),
                new Color(0.7f, 0.7f, 0.7f)  // Gray stone
            );
            baseObj.transform.SetParent(tower.transform);

            // Tower middle
            var middleObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cylinder,
                "Tower_Middle",
                position + Vector3.up * 1.5f,
                new Vector3(1.5f, 3f, 1.5f),
                new Color(0.6f, 0.6f, 0.6f)
            );
            middleObj.transform.SetParent(tower.transform);

            // Tower top
            var topObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cylinder,
                "Tower_Top",
                position + Vector3.up * 4f,
                new Vector3(1.8f, 0.5f, 1.8f),
                new Color(0.5f, 0.5f, 0.5f)
            );
            topObj.transform.SetParent(tower.transform);

            LogHelper.Log($"Built tower at {position}");
        }

        private void BuildWall(Vector3 position)
        {
            var wall = new GameObject("AI_Wall");
            wall.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            wall.transform.position = position;
            wall.tag = TagNames.AISpawned;

            // Create wall segments
            for (int i = 0; i < 5; i++)
            {
                var segmentPos = position + Vector3.right * i * 1.5f;
                var segment = SandboxManager.Instance.CreateSimpleObject(
                    PrimitiveType.Cube,
                    $"Wall_Segment_{i}",
                    segmentPos,
                    new Vector3(1.4f, 3f, 0.5f),
                    new Color(0.6f, 0.6f, 0.6f)  // Stone gray
                );
                segment.transform.SetParent(wall.transform);
            }

            LogHelper.Log($"Built wall at {position}");
        }

        private void BuildBridge(Vector3 position)
        {
            var bridge = new GameObject("AI_Bridge");
            bridge.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            bridge.transform.position = position;
            bridge.tag = TagNames.AISpawned;

            // Bridge deck
            var deck = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "Bridge_Deck",
                position + Vector3.up * 0.2f,
                new Vector3(8f, 0.3f, 2f),
                new Color(0.5f, 0.3f, 0.1f)  // Wood brown
            );
            deck.transform.SetParent(bridge.transform);

            // Bridge supports
            for (int i = -1; i <= 1; i++)
            {
                var supportPos = position + Vector3.right * i * 3f;
                var support = SandboxManager.Instance.CreateSimpleObject(
                    PrimitiveType.Cylinder,
                    $"Bridge_Support_{i}",
                    supportPos,
                    new Vector3(0.3f, 2f, 0.3f),
                    new Color(0.4f, 0.2f, 0.1f)
                );
                support.transform.SetParent(bridge.transform);
            }

            LogHelper.Log($"Built bridge at {position}");
        }

        private void BuildCastle(Vector3 position)
        {
            var castle = new GameObject("AI_Castle");
            castle.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            castle.transform.position = position;
            castle.tag = TagNames.AISpawned;

            // Main keep
            var keep = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                "Castle_Keep",
                position,
                new Vector3(4f, 4f, 4f),
                new Color(0.6f, 0.6f, 0.6f)
            );
            keep.transform.SetParent(castle.transform);

            // Corner towers
            Vector3[] towerPositions = {
                position + new Vector3(-3f, 0, -3f),
                position + new Vector3(3f, 0, -3f),
                position + new Vector3(-3f, 0, 3f),
                position + new Vector3(3f, 0, 3f)
            };

            for (int i = 0; i < towerPositions.Length; i++)
            {
                var tower = SandboxManager.Instance.CreateSimpleObject(
                    PrimitiveType.Cylinder,
                    $"Castle_Tower_{i}",
                    towerPositions[i],
                    new Vector3(1.5f, 5f, 1.5f),
                    new Color(0.5f, 0.5f, 0.5f)
                );
                tower.transform.SetParent(castle.transform);
            }

            LogHelper.Log($"Built castle at {position}");
        }

        private void BuildVillage(Vector3 position)
        {
            var village = new GameObject("AI_Village");
            village.transform.SetParent(SandboxManager.Instance.EnvironmentRoot);
            village.transform.position = position;
            village.tag = TagNames.AISpawned;

            // Build multiple houses in a village layout
            Vector3[] housePositions = {
                position + new Vector3(-4f, 0, -2f),
                position + new Vector3(0f, 0, -4f),
                position + new Vector3(4f, 0, -2f),
                position + new Vector3(-2f, 0, 2f),
                position + new Vector3(2f, 0, 4f)
            };

            for (int i = 0; i < housePositions.Length; i++)
            {
                BuildHouseAt(housePositions[i], village.transform, $"Village_House_{i}");
            }

            // Add a well in the center
            var well = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cylinder,
                "Village_Well",
                position,
                new Vector3(1f, 1.5f, 1f),
                new Color(0.4f, 0.4f, 0.4f)
            );
            well.transform.SetParent(village.transform);

            LogHelper.Log($"Built village at {position}");
        }

        private void BuildHouseAt(Vector3 position, Transform parent, string name)
        {
            var house = new GameObject(name);
            house.transform.SetParent(parent);
            house.transform.position = position;

            // Simple house structure
            var baseObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                $"{name}_Base",
                position,
                new Vector3(2f, 1.5f, 2f),
                new Color(Random.Range(0.6f, 0.9f), Random.Range(0.4f, 0.7f), Random.Range(0.2f, 0.5f))
            );
            baseObj.transform.SetParent(house.transform);

            var roofObj = SandboxManager.Instance.CreateSimpleObject(
                PrimitiveType.Cube,
                $"{name}_Roof",
                position + Vector3.up * 1.7f,
                new Vector3(2.3f, 0.3f, 2.3f),
                Color.red
            );
            roofObj.transform.SetParent(house.transform);
        }

        private Vector3 GetBuildPosition(string[] positionParams)
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
                    case "left": return new Vector3(-5, 0, 0);
                    case "right": return new Vector3(5, 0, 0);
                    case "center": return new Vector3(0, 0, 0);
                    case "front": return new Vector3(0, 0, 5);
                    case "back": return new Vector3(0, 0, -5);
                    case "northwest": return new Vector3(-5, 0, 5);
                    case "northeast": return new Vector3(5, 0, 5);
                    case "southwest": return new Vector3(-5, 0, -5);
                    case "southeast": return new Vector3(5, 0, -5);
                }
            }

            // Random position for larger structures
            return new Vector3(Random.Range(-6f, 6f), 0, Random.Range(-6f, 6f));
        }
    }
}