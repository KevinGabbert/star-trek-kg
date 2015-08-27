using System;
using System.Collections.Generic;

namespace StarTrek_KG.TypeSafeEnums
{
    public sealed class ScanRenderType
    {
        private readonly string name;
        private readonly int value;

        public static readonly ScanRenderType SingleLine = new ScanRenderType(1, "SingleLine");
        public static readonly ScanRenderType DoubleSingleLine = new ScanRenderType(2, "DoubleSingleLine");

        private static Dictionary<string, ScanRenderType> instance = new Dictionary<string, ScanRenderType>();

        private ScanRenderType(int value, string name)
        {
            this.name = name;
            this.value = value;

            if (instance == null)
            {
                instance = new Dictionary<string, ScanRenderType>();
            }

            instance[name] = this;
        }

        public override string ToString()
        {
            return name;
        }

        public static explicit operator ScanRenderType(string str)
        {
            ScanRenderType result;
            if (instance.TryGetValue(str, out result))
                return result;
            else
                throw new InvalidCastException();
        }
    }
}
