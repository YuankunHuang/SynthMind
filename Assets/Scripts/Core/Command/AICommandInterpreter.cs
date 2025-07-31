using System.Text.RegularExpressions;

namespace YuankunHuang.Unity.Core
{
    /// <summary>
    /// Processes natural language
    /// </summary>
    public class AICommandInterpreter
    {
        private ICommandManager _commandManager;

        public AICommandInterpreter(ICommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public bool TryProcessNaturalLanguage(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var command = InterpretInput(input.ToLower());
            if (!string.IsNullOrEmpty(command))
            {
                return _commandManager.TryExecuteCommand(command);
            }

            return false;
        }

        public string InterpretInput(string input)
        {
            // tree planting
            if (Regex.IsMatch(input, @"(plant|grow|create|spawn|make).*tree"))
            {
                var pos = ExtractPosition(input);
                return pos != null ? $"spawn tree {pos}" : $"spawn tree";
            }

            // character movement
            if (Regex.IsMatch(input, @"(move|walk|go).*to"))
            {
                var pos = ExtractPosition(input);
                return pos != null ? $"move to {pos}" : "move random";
            }

            // object creation patterns
            if (Regex.IsMatch(input, @"(create|spawn|make|build).*house"))
            {
                var pos = ExtractPosition(input);
                return pos != null ? $"spawn house {pos}" : "spawn house";
            }

            // clear/reset
            if (Regex.IsMatch(input, @"(clear|remove|delete|clean)"))
            {
                return "clear all";
            }

            return null;
        }

        private string ExtractPosition(string input)
        {
            var match = Regex.Match(input, @"(?|at|to|position)\s+(\d)\s+(\d)");
            if (match.Success)
            {
                return $"{match.Groups[1].Value} {match.Groups[2].Value}";
            }

            if (input.Contains("left"))
            {
                return "left";
            }

            if (input.Contains("right"))
            {
                return "right";
            }

            if (input.Contains("center"))
            {
                return "center";
            }

            return null;
        }
    }
}