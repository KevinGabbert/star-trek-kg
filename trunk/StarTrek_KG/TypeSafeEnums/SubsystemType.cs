using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class SubsystemType
    {
        private readonly String name;
        private readonly int value;

        public static readonly SubsystemType Debug = new SubsystemType(1, "Debug");
        public static readonly SubsystemType Computer = new SubsystemType(2, "Computer");
        public static readonly SubsystemType Shields = new SubsystemType(3, "Shields"); //5
        public static readonly SubsystemType Navigation = new SubsystemType(4, "Navigation");
        public static readonly SubsystemType ShortRangeScan = new SubsystemType(5, "ShortRangeScan");
        public static readonly SubsystemType LongRangeScan = new SubsystemType(6, "LongRangeScan");
        public static readonly SubsystemType Torpedoes = new SubsystemType(7, "Torpedoes");
        public static readonly SubsystemType Phasers = new SubsystemType(8, "Phasers");

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
    }
}
