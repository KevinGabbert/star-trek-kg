using System;

namespace StarTrek_KG.Extensions
{
    public static class intExtensions
    {
        public static string FormatForLRS(this int count)
        {
            if (count < 0)
            {
                return "-";
            }

            if(count > 15)
            {
                return "*";
            }

            if (count > -1 && count <= 15)
            {
                return count.ToString("X");
            }
            else
            {
                throw new ArgumentException("unexpected");
            }
        }

        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }

        //public static bool IsDouble(this string s)
        //{
        //    double output;
        //    return Double.TryParse(s, out output);
        //}
    }
}
