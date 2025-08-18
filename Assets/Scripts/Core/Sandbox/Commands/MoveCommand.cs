using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.CommandCore
{
    public class MoveCommand : IGameCommand
    {
        public string CommandName => "move";
        public string Description => "Moves the AI character. Usage: move [direction/position] or move [x] [z]";

        public bool CanExecute(string[] parameters)
        {
            return SandboxCharacterManager.Instance != null;
        }

        public void Execute(string[] parameters)
        {
            var characterManager = SandboxCharacterManager.Instance;
            if (characterManager == null)
            {
                LogHelper.LogError("No character manager found");
                return;
            }

            var character = characterManager.GetAICharacter();
            if (character == null)
            {
                LogHelper.LogError("No AI character found");
                return;
            }

            Vector3 targetPosition = GetTargetPosition(parameters, character.transform.position);
            float duration = GetMoveDuration(parameters);

            characterManager.MoveCharacterTo(targetPosition, duration);
            LogHelper.Log($"Moving AI character to {targetPosition}");
        }

        private Vector3 GetTargetPosition(string[] parameters, Vector3 currentPosition)
        {
            if (parameters.Length == 0)
            {
                // No parameters - move randomly
                return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            }

            // Try to parse as coordinates first
            if (parameters.Length >= 2)
            {
                if (float.TryParse(parameters[0], out float x) &&
                    float.TryParse(parameters[1], out float z))
                {
                    return new Vector3(x, 0, z);
                }
            }

            // Parse as direction/command
            var direction = parameters[0].ToLower();
            switch (direction)
            {
                case "left":
                    return currentPosition + Vector3.left * 3f;
                case "right":
                    return currentPosition + Vector3.right * 3f;
                case "forward":
                case "front":
                    return currentPosition + Vector3.forward * 3f;
                case "backward":
                case "back":
                    return currentPosition + Vector3.back * 3f;
                case "center":
                case "home":
                    return Vector3.zero;
                case "random":
                    return new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));

                // Diagonal movements
                case "northeast":
                case "ne":
                    return currentPosition + new Vector3(3f, 0, 3f);
                case "northwest":
                case "nw":
                    return currentPosition + new Vector3(-3f, 0, 3f);
                case "southeast":
                case "se":
                    return currentPosition + new Vector3(3f, 0, -3f);
                case "southwest":
                case "sw":
                    return currentPosition + new Vector3(-3f, 0, -3f);

                // Distance modifiers
                case "far":
                    if (parameters.Length > 1)
                    {
                        return GetTargetPosition(parameters.Skip(1).ToArray(), currentPosition) * 2f;
                    }
                    return new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                case "near":
                    if (parameters.Length > 1)
                    {
                        return Vector3.Lerp(currentPosition, GetTargetPosition(parameters.Skip(1).ToArray(), currentPosition), 0.5f);
                    }
                    return currentPosition + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

                // Absolute positions
                case "corner":
                    Vector3[] corners = {
                        new Vector3(-8f, 0, -8f),
                        new Vector3(8f, 0, -8f),
                        new Vector3(8f, 0, 8f),
                        new Vector3(-8f, 0, 8f)
                    };
                    return corners[Random.Range(0, corners.Length)];

                case "edge":
                    // Move to a random edge position
                    int edge = Random.Range(0, 4);
                    return edge switch
                    {
                        0 => new Vector3(Random.Range(-8f, 8f), 0, -8f), // Bottom edge
                        1 => new Vector3(8f, 0, Random.Range(-8f, 8f)),  // Right edge
                        2 => new Vector3(Random.Range(-8f, 8f), 0, 8f),  // Top edge
                        3 => new Vector3(-8f, 0, Random.Range(-8f, 8f)), // Left edge
                        _ => Vector3.zero
                    };

                default:
                    LogHelper.LogWarning($"Unknown movement direction: {direction}. Using random position.");
                    return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            }
        }

        private float GetMoveDuration(string[] parameters)
        {
            // Check for speed modifiers
            foreach (var param in parameters)
            {
                switch (param.ToLower())
                {
                    case "fast":
                    case "quick":
                    case "quickly":
                        return 1f;
                    case "slow":
                    case "slowly":
                        return 4f;
                    case "instant":
                    case "immediately":
                        return 0.1f;
                }
            }

            return 2f; // Default duration
        }
    }
}