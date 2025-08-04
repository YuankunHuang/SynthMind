using System.Text.RegularExpressions;

namespace YuankunHuang.Unity.Core
{
    public static class NaturalLanguagePatterns
    {
        // tree
        public static readonly string[] TreePatterns =
        {
            @"(plant|grow|create|spawn|make|add).*tree",
            @"tree.*(plant|grow|create|spawn|make|add)",
            @"(put|place).*tree",
            @"grow.*forest"
        };

        // character
        public static readonly string[] MovePatterns =
        {
            @"(move|walk|go|send).*character",
            @"character.*(move|walk|go|to)",
            @"(tell|ask|command).*(character|him|her|it).*(move|go|walk)",
            @"(navigate|direct).*character"
        };

        // building
        public static readonly string[] BuildingPatterns =
        {
            @"(build|create|construct|make|spawn|add).*(house|building|structure|home)",
            @"(house|building|structure|home).*(build|create|construct|make|spawn|add)",
            @"(put|place).*(house|building|structure)"
        };

        // clear
        public static readonly string[] ClearPatterns =
        {
            @"(clear|remove|delete|clean|reset|destroy).*(all|everything|scene)",
            @"(all|everything|scene).*(clear|remove|delete|clean|reset|destroy)",
            @"start.*over",
            @"clean.*up"
        };

        // position extraction
        public static readonly string PositionPattern = @"(?:at|to|position|coordinates?)\s+(-?\d+)(?:\s*,?\s*|\s+)(-?\d+)";
        public static readonly string DirectionPattern = @"\b(left|right|center|centre|front|back|forward|behind|north|south|east|west)\b";
    }

    public class NaturalLanguageProcessor
    {
        public static CommandParseResult ParseNaturalInput(string input)
        {
            LogHelper.LogError($"ParseNaturalInput - input: {input}");

            if (string.IsNullOrEmpty(input))
            {
                return new CommandParseResult { Success = false };
            }

            var normalizedInput = input.ToLower().Trim();

            if (MatchesAnyPattern(normalizedInput, NaturalLanguagePatterns.TreePatterns))
            {
                LogHelper.LogError($"Matched TreePatterns");

                return ParseTreeCommand(normalizedInput);
            }

            if (MatchesAnyPattern(normalizedInput, NaturalLanguagePatterns.MovePatterns))
            {
                LogHelper.LogError($"Matched MovePatterns");

                return ParseMoveCommand(normalizedInput);
            }

            if (MatchesAnyPattern(normalizedInput, NaturalLanguagePatterns.BuildingPatterns))
            {
                LogHelper.LogError($"Matched BuildingPatterns");

                return ParseBuildingCommand(normalizedInput);
            }

            if (MatchesAnyPattern(normalizedInput, NaturalLanguagePatterns.ClearPatterns))
            {
                LogHelper.LogError($"Matched ClearPatterns");

                return ParseClearCommand(normalizedInput);
            }

            return new CommandParseResult { Success = false };
        }

        private static bool MatchesAnyPattern(string input, string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(pattern, input))
                {
                    return true;
                }
            }

            return false;
        }

        #region Parser
        private static CommandParseResult ParseTreeCommand(string input)
        {
            var pos = ExtractPosition(input);
            var command = pos.HasValue
                ? $"spawn tree {pos.Value.x} {pos.Value.z}"
                : "spawn tree";

            LogHelper.LogError($"ParseTreeCommand - pos: {pos} | command: {command}");

            return new CommandParseResult
            {
                Success = true,
                Command = command,
                OriginalInput = input,
            };
        }

        private static CommandParseResult ParseMoveCommand(string input)
        {
            var pos = ExtractPosition(input);
            var command = pos.HasValue
                ? $"move {pos.Value.x} {pos.Value.z}"
                : "move random";

            LogHelper.LogError($"ParseMoveCommand - pos: {pos} | command: {command}");

            return new CommandParseResult
            {
                Success = true,
                Command = command,
                OriginalInput = input,
            };
        }

        private static CommandParseResult ParseBuildingCommand(string input)
        {
            var pos = ExtractPosition(input);
            var command = pos.HasValue
                ? $"build house {pos.Value.x} {pos.Value.z}"
                : "build house";

            LogHelper.LogError($"ParseBuildingCommand - pos: {pos} | command: {command}");

            return new CommandParseResult
            {
                Success = true,
                Command = command,
                OriginalInput = input,
            };
        }

        private static CommandParseResult ParseClearCommand(string input)
        {
            return new CommandParseResult
            {
                Success = true,
                Command = "clear all",
                OriginalInput = input,
            };
        }
        #endregion

        #region General
        private static (float x, float z)? ExtractPosition(string input)
        {
            var match = Regex.Match(input, NaturalLanguagePatterns.PositionPattern);

            LogHelper.LogError($"Matching Position Pattern -> {match}");

            // try coordinate
            if (match.Success)
            {
                if (float.TryParse(match.Groups[1].Value, out var x) &&
                    float.TryParse(match.Groups[2].Value, out var z))
                {
                    return (x, z);
                }
            }

            // try direction
            var directionMatch = Regex.Match(input, NaturalLanguagePatterns.DirectionPattern);

            LogHelper.LogError($"Matching DirectionPattern -> {directionMatch}");

            if (directionMatch.Success)
            {
                var direction = directionMatch.Groups[1].Value.ToLower();
                return direction switch
                {
                    "left" => (-3f, 0),
                    "right" => (3f, 0),
                    "center" or "centre" => (0, 0),
                    "front" or "forward" => (0f, 3f),
                    "back" or "backward" or "behind" => (0f, -3f),
                    "north" => (0, 5f),
                    "south" => (0, -5f),
                    "east" => (5f, 0),
                    "west" => (-5f, 0),
                    _ => null
                };
            }

            return null;
        }
        #endregion
    }

    public struct CommandParseResult
    {
        public bool Success;
        public string Command;
        public string OriginalInput;
    }
}
