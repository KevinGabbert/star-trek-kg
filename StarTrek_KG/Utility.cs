
using System;
using System.Collections.Generic;

namespace StarTrek_KG
{
    public static class Utility
    {
        public static Random Random = new Random(Convert.ToInt32(DateTime.Today.Millisecond + DateTime.Today.Minute));

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}
