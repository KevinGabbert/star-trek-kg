using System;

namespace StarTrek_KG.Extensions
{
    public static class intExtensions
    {
        //public static string FormatForLRS(this int count)
        //{
        //    if (count < 0)
        //    {
        //        return "~";
        //    }

        //    if(count > 15)
        //    {
        //        return "*";
        //    }

        //    if (count > -1 && count <= 15)
        //    {
        //        return count.ToString("X");
        //    }
        //    else
        //    {
        //        throw new ArgumentException("unexpected");
        //    }
        //}

        public static string FormatForLRS(this int? count)
        {
            if (count == null)
            {
                return "-";
            }

            if (count < 0)
            {
                return "~";
            }

            if (count > 15)
            {
                return "*";
            }

            if (count > -1 && count <= 15)
            {
                return count.ToString();
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
    }
}
