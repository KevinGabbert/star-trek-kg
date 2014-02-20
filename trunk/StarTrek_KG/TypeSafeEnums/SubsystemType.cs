using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class SubsystemType
    {
        private readonly String name;
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
        public static readonly SubsystemType CombinedRangeScan = new SubsystemType(5, "Combined Range Scan");

        private static Dictionary<string, SubsystemType> instance = new Dictionary<string, SubsystemType>();

        private SubsystemType(int value, String name)
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

        public override String ToString()
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

            switch (subsystemToFix)
            {
                case "nav":
                    returnVal = SubsystemType.Navigation;
                    break;

                case "srs":
                    returnVal = SubsystemType.ShortRangeScan;
                    break;

                case "lrs":
                    returnVal = SubsystemType.LongRangeScan;
                    break;

                case "crs":
                    returnVal = SubsystemType.CombinedRangeScan;
                    break;

                case "pha":
                    returnVal = SubsystemType.Phasers;
                    break;

                case "tor":
                    returnVal = SubsystemType.Torpedoes;
                    break;

                case "she":
                    returnVal = SubsystemType.Shields;
                    break;

                case "com":
                    returnVal = SubsystemType.Computer;
                    break;

                //case "dmg":
                //    returnVal = SubsystemType.DamageControl;
                //    break;

                //case "dbg":
                //    returnVal = SubsystemType.Debug;
                //    break;
            }

            return returnVal;
        }
    }
}
