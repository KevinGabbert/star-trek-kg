using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class FactionName
    {
        private readonly string name;
        private readonly int value;

        public static readonly FactionName Federation = new FactionName(1, "Federation");
        public static readonly FactionName Klingon = new FactionName(2, "Klingon");
        public static readonly FactionName Romulan = new FactionName(3, "Romulan");
        public static readonly FactionName Vulcan = new FactionName(4, "Vulcan");
        public static readonly FactionName Gorn = new FactionName(5, "Gorn");
        public static readonly FactionName Other = new FactionName(6, "Other");
        public static readonly FactionName TestFaction = new FactionName(7, "TestFaction");

        private static Dictionary<string, FactionName> instance = new Dictionary<string, FactionName>();

        private FactionName(int value, string name)
        {
            this.name = name;
            this.value = value;

            if (instance == null)
            {
                instance = new Dictionary<string, FactionName>();
                // instance.Add(name, null);
            }

            instance[name] = this;
        }

        public override string ToString()
        {
            return name;
        }

        public static explicit operator FactionName(string str)
        {
            FactionName result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
    }
}
