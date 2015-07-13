using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class SubsystemType
    {
        private readonly string name;
        private readonly int value;

        public static readonly SubsystemType None = new SubsystemType(1, "None");
        public static readonly SubsystemType Debug = new SubsystemType(1, "Debug");
        public static readonly SubsystemType Computer = new SubsystemType(2, "Computer");
        public static readonly SubsystemType Shields = new SubsystemType(3, "Shields"); //5
        public static readonly SubsystemType Navigation = new SubsystemType(4, "Navigation");
        public static readonly SubsystemType ShortRangeScan = new SubsystemType(5, "Short Range Scan");
        public static readonly SubsystemType LongRangeScan = new SubsystemType(6, "Long Range Scan");
        public static readonly SubsystemType Torpedoes = new SubsystemType(7, "Photon Torpedoes");
        public static readonly SubsystemType Phasers = new SubsystemType(8, "Phasers");
        public static readonly SubsystemType DamageControl = new SubsystemType(9, "Damage Control");
        public static readonly SubsystemType CombinedRangeScan = new SubsystemType(10, "Combined Range Scan");
        public static readonly SubsystemType Disruptors = new SubsystemType(11, "Disruptors");
        public static readonly SubsystemType ImmediateRangeScan = new SubsystemType(12, "Immediate Range Scan");

        private static Dictionary<string, SubsystemType> instance = new Dictionary<string, SubsystemType>();

        private SubsystemType(int value, string name)
        {
            this.name = name;
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
            return name;
        }

        public static explicit operator SubsystemType(string str)
        {
            SubsystemType result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }

        internal static SubsystemType GetFromAbbreviation(string subsystemToFix)
        {
            SubsystemType returnVal = SubsystemType.None;

            if (subsystemToFix == Menu.nav.ToString())
            {
                returnVal = SubsystemType.Navigation;
            }
            else if (subsystemToFix == Menu.irs.ToString())
            {
                returnVal = SubsystemType.ImmediateRangeScan;
            }
            else if (subsystemToFix == Menu.srs.ToString())
            {
                returnVal = SubsystemType.ShortRangeScan;
            }
            else if (subsystemToFix == Menu.lrs.ToString())
            {
                returnVal = SubsystemType.LongRangeScan;
            }
            else if (subsystemToFix == Menu.crs.ToString())
            {
                returnVal = SubsystemType.CombinedRangeScan;
            }
            else if (subsystemToFix == Menu.pha.ToString())
            {
                returnVal = SubsystemType.Phasers;
            }
            else if (subsystemToFix == Menu.tor.ToString())
            {
                returnVal = SubsystemType.Torpedoes;
            }
            else if (subsystemToFix == Menu.she.ToString())
            {
                returnVal = SubsystemType.Shields;
            }
            else if (subsystemToFix == Menu.com.ToString())
            {
                returnVal = SubsystemType.Computer;
            }

            return returnVal;
        }
    }
}
