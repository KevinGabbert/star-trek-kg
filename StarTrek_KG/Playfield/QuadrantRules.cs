using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Playfield
{
    public static class QuadrantRules
    {
        public static string GetQuadrantName(IMap map, int sectorX, int sectorY)
        {
            var half = Math.Max(1, DEFAULTS.SECTOR_MAX / 2);
            var alpha = GetSetting(map, "QuadrantAlphaName", "Alpha");
            var beta = GetSetting(map, "QuadrantBetaName", "Beta");
            var gamma = GetSetting(map, "QuadrantGammaName", "Gamma");
            var delta = GetSetting(map, "QuadrantDeltaName", "Delta");

            if (sectorY < half)
            {
                return sectorX < half ? alpha : beta;
            }

            return sectorX < half ? gamma : delta;
        }

        public static IEnumerable<FactionName> GetHostileFactionsForSector(IMap map, int sectorX, int sectorY)
        {
            var quadrantName = GetQuadrantName(map, sectorX, sectorY);
            var settingKey = $"Quadrant{quadrantName}HostileFactions";
            var defaults = quadrantName switch
            {
                "Alpha" => "Cardassian,Ferengi,Bajoran",
                "Beta" => "Klingon,Romulan",
                "Gamma" => "Dominion",
                "Delta" => "Borg,Kazon",
                _ => "Klingon"
            };

            return GetFactionList(map, settingKey, defaults);
        }

        public static IEnumerable<FactionName> GetFriendlyFactionsForSector(IMap map, int sectorX, int sectorY)
        {
            var quadrantName = GetQuadrantName(map, sectorX, sectorY);
            var settingKey = $"Quadrant{quadrantName}FriendlyFactions";
            var defaults = quadrantName switch
            {
                "Alpha" => "Federation",
                "Beta" => "Vulcan",
                _ => string.Empty
            };

            return GetFactionList(map, settingKey, defaults);
        }

        public static FactionName ChooseHostileFactionForSector(IMap map, int sectorX, int sectorY)
        {
            var primary = GetHostileFactionsForSector(map, sectorX, sectorY).ToList();
            var crossQuadrantChance = GetIntSetting(map, "CrossQuadrantHostileSpawnPercent", 2);
            if (Utility.Utility.Random.Next(100) < crossQuadrantChance)
            {
                var all = GetAllConfiguredHostileFactions(map).ToList();
                if (all.Count > 0)
                {
                    return all[Utility.Utility.Random.Next(all.Count)];
                }
            }

            if (primary.Count == 0)
            {
                return FactionName.Klingon;
            }

            return primary[Utility.Utility.Random.Next(primary.Count)];
        }

        public static string GetRandomShipNameForFaction(IMap map, FactionName faction)
        {
            try
            {
                var shipNames = map?.Config?.ShipNames(faction)?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
                if (shipNames != null && shipNames.Count > 0)
                {
                    return shipNames[Utility.Utility.Random.Next(shipNames.Count)].Trim();
                }
            }
            catch
            {
            }

            return $"Unknown {faction}";
        }

        public static int GetFriendlyShipsPerFaction(IMap map)
        {
            return GetIntSetting(map, "QuadrantFriendlyShipsPerFaction", 1);
        }

        public static int GetFriendlyMovePercent(IMap map)
        {
            return GetIntSetting(map, "QuadrantFriendlyMovePercent", 60);
        }

        public static int GetFriendlyCrossQuadrantMovePercent(IMap map)
        {
            return GetIntSetting(map, "QuadrantFriendlyCrossQuadrantMovePercent", 10);
        }

        public static bool IsFriendlyFaction(IMap map, FactionName faction)
        {
            try
            {
                var detail = map?.Config?.Factions?[faction.ToString()];
                return detail != null && string.Equals(detail.allegiance, "Friendly", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return faction == FactionName.Federation || faction == FactionName.Vulcan;
            }
        }

        public static string GetQuadrantLabel(IMap map, int sectorX, int sectorY)
        {
            return $"{GetQuadrantName(map, sectorX, sectorY)} Quadrant";
        }

        public static string GetQuadrantSymbol(IMap map, int sectorX, int sectorY)
        {
            return GetQuadrantSymbol(GetQuadrantName(map, sectorX, sectorY));
        }

        public static string GetQuadrantSymbol(string quadrantName)
        {
            switch ((quadrantName ?? string.Empty).Trim())
            {
                case "Alpha":
                    return "Α";
                case "Beta":
                    return "Β";
                case "Gamma":
                    return "Γ";
                case "Delta":
                    return "Δ";
                default:
                    return "?";
            }
        }

        private static IEnumerable<FactionName> GetAllConfiguredHostileFactions(IMap map)
        {
            return new[]
            {
                FactionName.Cardassian,
                FactionName.Ferengi,
                FactionName.Bajoran,
                FactionName.Klingon,
                FactionName.Romulan,
                FactionName.Dominion,
                FactionName.Borg,
                FactionName.Kazon
            }.Where(f => !IsFriendlyFaction(map, f));
        }

        private static IEnumerable<FactionName> GetFactionList(IMap map, string settingKey, string fallback)
        {
            var setting = GetSetting(map, settingKey, fallback);
            return (setting ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(ParseFaction)
                .Where(f => f != null)
                .ToList();
        }

        private static FactionName ParseFaction(string value)
        {
            try
            {
                return (FactionName)value;
            }
            catch
            {
                return null;
            }
        }

        private static string GetSetting(IMap map, string key, string fallback)
        {
            try
            {
                var configured = map?.Config?.GetSetting<string>(key);
                return string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();
            }
            catch
            {
                return fallback;
            }
        }

        private static int GetIntSetting(IMap map, string key, int fallback)
        {
            try
            {
                return map?.Config?.GetSetting<int>(key) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
