
using System;
using System.Collections.Generic;
using StarTrek_KG.Playfield;

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

        //calculates distance traveled mostly to be able to have a value for diagonal distance, as straight line calculation doesnt need all this effort.
        public static double Distance(double startX, double startY, double destinationX, double destinationY)
        {
            var distanceTraveledX = destinationX - startX;
            var distanceTraveledY = destinationY - startY;

            //X squared + Y Squared = Z squared. (Pythagorean Theorem), then Square Root to solve: 
            return Math.Sqrt(distanceTraveledX * distanceTraveledX +
                             distanceTraveledY * distanceTraveledY);
        }

        public static double ComputeDirection(int x1, int y1, int x2, int y2)
        {
            //Todo:  to place this in the right place, resolve StarbaseCalculator

            double direction;
            if (x1 == x2)
            {
                direction = y1 < y2 ? 7 : 3;
            }
            else if (y1 == y2)
            {
                direction = x1 < x2 ? 1 : 5;
            }
            else
            {
                double dy = Math.Abs(y2 - y1);
                double dx = Math.Abs(x2 - x1);
                var angle = Math.Atan2(dy, dx);
                if (x1 < x2)
                {
                    if (y1 < y2)
                    {
                        direction = 9.0 - 4.0 * angle / Math.PI;
                    }
                    else
                    {
                        direction = 1.0 + 4.0 * angle / Math.PI;
                    }
                }
                else
                {
                    if (y1 < y2)
                    {
                        direction = 5.0 + 4.0 * angle / Math.PI;
                    }
                    else
                    {
                        direction = 5.0 - 4.0 * angle / Math.PI;
                    }
                }
            }
            return direction;
        }

        public static double ComputeAngle(Map map, double direction) // todo: can this be refactored with nav computeangle?
        {
            var angle = -(Math.PI * (direction - 1.0) / 4.0);
            if ((Random).Next(3) == 0)
            {
                angle += ((1.0 - 2.0 * (Random).NextDouble()) * Math.PI * 2.0) * 0.03;
            }
            return angle;
        }

        //starbase position info (there needs to be a starbase object)
        public static int starbaseX;
        public static int starbaseY;

        //public void StarbaseCalculator()
        //{
        //    var location = Navigation.For(this.Playership);
        //    //if (StarTrek_KG.Quadrants.Get(this, location.quadrantX, location.quadrantY).Starbase)
        //    //{
        //    //    Console.WriteLine("Starbase in sector [{0},{1}].", (starbaseX + 1), (starbaseY + 1));
        //    //    Console.WriteLine("Direction: {0:#.##}", Map.ComputeDirection(location.sectorX, location.sectorY, starbaseX, starbaseY));
        //    //    Console.WriteLine("Distance:  {0:##.##}", Distance(location.sectorX, location.sectorY, starbaseX, starbaseY) / Constants.SECTOR_MAX);
        //    //}
        //    //else
        //    //{
        //    //    Output.Write("There are no starbases in this quadrant.");
        //    //}
        //}
    }
}
