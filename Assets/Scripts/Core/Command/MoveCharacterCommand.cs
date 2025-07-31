using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class MoveCharacterCommand : IGameCommand
    {
        public string CommandName => "move";
        public string Description => "Moves the AI character. Usage: move [position]";

        public bool CanExecute(string[] parameters)
        {
            return true; // Can always execute with any parameters
        }

        public void Execute(string[] parameters)
        {
            var character = CharacterManager.Instance?.GetAICharacter();
            if (character == null)
            {
                LogHelper.LogError("No AI character found to move");
                return;
            }

            Vector3 targetPosition = GetTargetPosition(parameters);
            CharacterManager.Instance.MoveCharacterTo(character, targetPosition);

            LogHelper.Log($"Moving AI character to {targetPosition}");
        }

        private Vector3 GetTargetPosition(string[] parameters)
        {
            if (parameters.Length >= 2)
            {
                if (float.TryParse(parameters[0], out float x) &&
                    float.TryParse(parameters[1], out float z))
                {
                    return new Vector3(x, 0, z);
                }
            }

            if (parameters.Length >= 1)
            {
                switch (parameters[0].ToLower())
                {
                    case "left": return new Vector3(-3, 0, 0);
                    case "right": return new Vector3(3, 0, 0);
                    case "center": return Vector3.zero;
                    case "random": return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                }
            }

            return new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        }
    }
}