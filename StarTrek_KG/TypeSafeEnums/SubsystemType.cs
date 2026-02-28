using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class SubsystemType
    {
        public string Name { get; }
        public string Abbreviation { get; }
        public readonly int value;

        public static readonly SubsystemType None = new SubsystemType(1, "None", "");
        public static readonly SubsystemType Debug = new SubsystemType(1, "Debug", "dbg");
        public static readonly SubsystemType Computer = new SubsystemType(2, "Computer", "com");
        public static readonly SubsystemType Shields = new SubsystemType(3, "Shields", "she"); //5
        public static readonly SubsystemType Navigation = new SubsystemType(4, "Navigation", "nav");
        public static readonly SubsystemType ShortRangeScan = new SubsystemType(5, "Short Range Scan", "srs");
        public static readonly SubsystemType LongRangeScan = new SubsystemType(6, "Long Range Scan", "lrs");
        public static readonly SubsystemType Torpedoes = new SubsystemType(7, "Photon Torpedoes", "pho");
        public static readonly SubsystemType Phasers = new SubsystemType(8, "Phasers", "pha");
        public static readonly SubsystemType DamageControl = new SubsystemType(9, "Damage Control", "dmg");
        public static readonly SubsystemType CombinedRangeScan = new SubsystemType(10, "Combined Range Scan", "crs");
        public static readonly SubsystemType Disruptors = new SubsystemType(11, "Disruptors", "dsr");
        public static readonly SubsystemType ImmediateRangeScan = new SubsystemType(12, "Immediate Range Scan", "irs");
        public static readonly SubsystemType Warp = new SubsystemType(13, "Warp Drive", "wrp");
        public static readonly SubsystemType Impulse = new SubsystemType(14, "Impulse Engines", "imp");

        //todo: refactor this to a new method to call war games.
        public static readonly SubsystemType WarGames = new SubsystemType(15, "War Games", "wgm");

        private static Dictionary<string, SubsystemType> instance = new Dictionary<string, SubsystemType>();

        private SubsystemType(int value, string name, string abbreviation)
        {
            this.Name = name;
            this.Abbreviation = abbreviation;
            this.value = value;

            if (instance == null)
            {
                instance = new Dictionary<string, SubsystemType>();
                // instance.Add(name, null);
            }

            instance[name] = this;
        }

        public override string ToString()
        {
            return Name;
        }

        public static explicit operator SubsystemType(string str)
        {
            SubsystemType result;
            if (instance.TryGetValue(str, out result))
            {
                return result;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        internal static SubsystemType GetFromAbbreviation(string subsystemAbbr)
        {
            SubsystemType returnVal = SubsystemType.None;

            if (subsystemAbbr == Menu.nav.ToString())
            {
                returnVal = SubsystemType.Navigation;
            }
            else if (subsystemAbbr == Menu.irs.ToString())
            {
                returnVal = SubsystemType.ImmediateRangeScan;
            }
            else if (subsystemAbbr == Menu.srs.ToString())
            {
                returnVal = SubsystemType.ShortRangeScan;
            }
            else if (subsystemAbbr == Menu.lrs.ToString())
            {
                returnVal = SubsystemType.LongRangeScan;
            }
            else if (subsystemAbbr == Menu.crs.ToString())
            {
                returnVal = SubsystemType.CombinedRangeScan;
            }
            else if (subsystemAbbr == Menu.pha.ToString())
            {
                returnVal = SubsystemType.Phasers;
            }
            else if (subsystemAbbr == Menu.tor.ToString())
            {
                returnVal = SubsystemType.Torpedoes;
            }
            else if (subsystemAbbr == Menu.she.ToString())
            {
                returnVal = SubsystemType.Shields;
            }
            else if (subsystemAbbr == Menu.com.ToString())
            {
                returnVal = SubsystemType.Computer;
            }

            return returnVal;
        }
    }
}
