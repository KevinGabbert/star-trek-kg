using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StarTrek_KG.Enums;

namespace StarTrek_KG.Commands
{
    public static class NaturalLanguageRouter
    {
        private static readonly Dictionary<string, int> DirectionMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"up", (int)NavDirection.Up},
            {"down", (int)NavDirection.Down},
            {"left", (int)NavDirection.Left},
            {"right", (int)NavDirection.Right},
            {"north", (int)NavDirection.Up},
            {"south", (int)NavDirection.Down},
            {"west", (int)NavDirection.Left},
            {"east", (int)NavDirection.Right},
            {"northwest", (int)NavDirection.LeftUp},
            {"north-east", (int)NavDirection.RightUp},
            {"northeast", (int)NavDirection.RightUp},
            {"southwest", (int)NavDirection.LeftDown},
            {"south-east", (int)NavDirection.RightDown},
            {"southeast", (int)NavDirection.RightDown}
        };

        private static readonly List<(Regex pattern, Func<Match, string> map)> Rules =
            new List<(Regex, Func<Match, string>)>
            {
                (new Regex(@"^(imp|impulse)\s+(?<dir>[1-8])\s+(?<dist>\d+)$", RegexOptions.IgnoreCase),
                    m => $"imp {m.Groups["dir"].Value} {m.Groups["dist"].Value}"),

                (new Regex(@"^(move|impulse)\s+(?<dir>north|south|east|west|up|down|left|right|northwest|northeast|southwest|southeast|north-east|south-east)\s+(?<dist>\d+)",
                           RegexOptions.IgnoreCase),
                    m => MapDirection(m.Groups["dir"].Value, m.Groups["dist"].Value, "imp")),

                (new Regex(@"^(warp)\s+(?<speed>\d+)\s+(course\s+)?(?<dir>[1-8])$",
                           RegexOptions.IgnoreCase),
                    m => $"wrp {m.Groups["dir"].Value} {m.Groups["speed"].Value}"),

                (new Regex(@"^(warp)\s+(?<speed>\d+)\s+(to\s+)?(?<dir>north|south|east|west|up|down|left|right|northwest|northeast|southwest|southeast|north-east|south-east)$",
                           RegexOptions.IgnoreCase),
                    m => MapDirection(m.Groups["dir"].Value, m.Groups["speed"].Value, "wrp")),

                (new Regex(@"^(set\s+)?course\s+(?<dir>[1-8])$", RegexOptions.IgnoreCase),
                    m => $"wrp {m.Groups["dir"].Value}"),

                (new Regex(@"^(set\s+speed|set\s+warp\s+speed|set\s+warp)\s+(?<speed>\d+)$", RegexOptions.IgnoreCase),
                    m => $"wrp speed {m.Groups["speed"].Value}"),

                (new Regex(@"^(engage\s+warp|engage)$", RegexOptions.IgnoreCase),
                    m => "wrp engage"),

                (new Regex(@"^(navigate|go)\s+to\s+object\s+(?<id>\d+)$", RegexOptions.IgnoreCase),
                    m => $"nto {m.Groups["id"].Value}"),

                (new Regex(@"^(target)\s+object\s+(?<id>\d+)$", RegexOptions.IgnoreCase),
                    m => $"toq {m.Groups["id"].Value}"),

                (new Regex(@"^(immediate\s+range\s+scan|irs)$", RegexOptions.IgnoreCase),
                    m => "irs"),

                (new Regex(@"^(short\s+range\s+scan|srs)$", RegexOptions.IgnoreCase),
                    m => "srs"),

                (new Regex(@"^(long\s+range\s+scan|lrs)$", RegexOptions.IgnoreCase),
                    m => "lrs"),

                (new Regex(@"^(combined\s+range\s+scan|crs)$", RegexOptions.IgnoreCase),
                    m => "crs"),

                (new Regex(@"^(fire|shoot)\s+phasers?\s+(?<amt>\d+)$", RegexOptions.IgnoreCase),
                    m => $"pha {m.Groups["amt"].Value}"),

                (new Regex(@"^(fire|shoot)\s+torpedo(es)?\s+(at\s+)?(?<x>\d+)\s+(?<y>\d+)$", RegexOptions.IgnoreCase),
                    m => $"tor {m.Groups["x"].Value} {m.Groups["y"].Value}"),

                (new Regex(@"^(raise|up)\s+shields?$", RegexOptions.IgnoreCase),
                    m => "she up"),

                (new Regex(@"^(lower|down|drop)\s+shields?$", RegexOptions.IgnoreCase),
                    m => "she down"),

                (new Regex(@"^(add|transfer|move)\s+(?<amt>\d+)\s+(to|into)\s+shields?$", RegexOptions.IgnoreCase),
                    m => $"she add {m.Groups["amt"].Value}"),

                (new Regex(@"^(computer|com)$", RegexOptions.IgnoreCase),
                    m => "com"),

                (new Regex(@"^(damage\s+control|repair\s+systems|dmg)$", RegexOptions.IgnoreCase),
                    m => "dmg")
            };

        public static string TryParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            string normalized = Regex.Replace(input.Trim(), @"\s+", " ");

            // If user already typed a known command prefix, leave it alone.
            string[] known = { "imp", "wrp", "nto", "irs", "srs", "lrs", "crs", "pha", "tor", "toq", "she", "com", "dmg", "dbg", "ver" };
            for (int i = 0; i < known.Length; i++)
            {
                if (normalized.StartsWith(known[i] + " ", StringComparison.OrdinalIgnoreCase) ||
                    normalized.Equals(known[i], StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            for (int i = 0; i < Rules.Count; i++)
            {
                Match match = Rules[i].pattern.Match(normalized);
                if (match.Success)
                {
                    return Rules[i].map(match);
                }
            }

            return null;
        }

        private static string MapDirection(string directionText, string valueText, string command)
        {
            int direction;
            if (!DirectionMap.TryGetValue(directionText.ToLowerInvariant(), out direction))
            {
                return null;
            }

            return $"{command} {direction} {valueText}";
        }
    }
}
