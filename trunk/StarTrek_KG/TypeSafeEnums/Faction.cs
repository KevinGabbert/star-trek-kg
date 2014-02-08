using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class Faction
    {
        private readonly String name;
        private readonly int value;

        public static readonly Faction Federation = new Faction(1, "Federation");
        public static readonly Faction Klingon = new Faction(2, "Klingon");
        public static readonly Faction Romulan = new Faction(3, "Romulan");
        public static readonly Faction Vulcan = new Faction(4, "Vulcan");
        public static readonly Faction Gorn = new Faction(5, "Gorn");
        public static readonly Faction Other = new Faction(6, "Other");
        public static readonly Faction TestFaction = new Faction(7, "TestFaction");

        private static Dictionary<string, Faction> instance = new Dictionary<string, Faction>();

        private Faction(int value, String name)
        {
            this.name = name;
            this.value = value;

            if (instance == null)
            {
                instance = new Dictionary<string, Faction>();
                // instance.Add(name, null);
            }

            instance[name] = this;
        }

        public override String ToString()
        {
            return name;
        }

        public static explicit operator Faction(string str)
        {
            Faction result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
    }
}
