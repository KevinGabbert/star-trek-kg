using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Utility
{
    public static class Utility
    {
        public static Stack<string> RandomGreekLetter; 
        public static Random Random = new Random(Guid.NewGuid().GetHashCode());

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            Random = new Random(Guid.NewGuid().GetHashCode());

            var rng = StarTrek_KG.Utility.Utility.Random;
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

        ////calculates distance traveled mostly to be able to have a value for diagonal distance, as straight line calculation doesnt need all this effort.
        ////todo: modify this to take location
        //public static double Distance(double startX, double startY, double destinationX, double destinationY)
        //{
        //    var distanceTraveledX = destinationX - startX;
        //    var distanceTraveledY = destinationY - startY;

        //    //X squared + Y Squared = Z squared. (Pythagorean Theorem), then Square Root to solve: 
        //    return Math.Sqrt(distanceTraveledX * distanceTraveledX +
        //                     distanceTraveledY * distanceTraveledY);
        //}

        public static int Distance(int startX, int startY, int destinationX, int destinationY)
        {
            var distanceTraveledX = destinationX - startX;
            var distanceTraveledY = destinationY - startY;

            //X squared + Y Squared = Z squared. (Pythagorean Theorem), then Square Root to solve: 
            return Convert.ToInt32(Math.Sqrt(distanceTraveledX * distanceTraveledX +
                             distanceTraveledY * distanceTraveledY));
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

        public static double ComputeAngle(double direction) // todo: can this be refactored with nav computeangle?
        {
            var angle = -(Math.PI * (direction - 1.0) / 4.0);

            if ((Random).Next(3) == 0)
            {
                angle += ((1.0 - 2.0 * (Utility.Random).NextDouble()) * Math.PI * 2.0) * 0.03;
            }

            return angle;
        }

        public static double ComputeBeamWeaponIntensity(double energyToPowerWeapon, double energyAdjustment, double distance, double deprecationRate)
        {
            return energyToPowerWeapon*(energyAdjustment - distance/deprecationRate);
        }

        //todo: move to Utility() object
        public static double ShootBeamWeapon(double energyToPowerWeapon, double distance, string deprecationRateConfigKey, string energyAdjustmentConfigKey, bool inNebula)
        {
            var deprecationRate = (new StarTrekKGSettings()).GetSetting<double>(deprecationRateConfigKey);
            var energyAdjustment = (new StarTrekKGSettings()).GetSetting<double>(energyAdjustmentConfigKey);

            double actualDeprecationRate;

            if (inNebula)
            {
                actualDeprecationRate = deprecationRate / 2;
            }
            else
            {
                actualDeprecationRate = deprecationRate;
            }

            double deliveredEnergy = StarTrek_KG.Utility.Utility.ComputeBeamWeaponIntensity(energyToPowerWeapon, energyAdjustment, distance, actualDeprecationRate);
            double actualDeliveredEnergy = (deliveredEnergy < 0) ? 0 : deliveredEnergy;

            return actualDeliveredEnergy;
        }

        public static void ResetGreekLetterStack()
        {
            var greekLetters = new List<string>();
            greekLetters.Add("ALPHA");
            greekLetters.Add("BETA");
            greekLetters.Add("GAMMA");
            greekLetters.Add("DELTA");
            greekLetters.Add("EPSILON");
            greekLetters.Add("ZETA");
            greekLetters.Add("ETA");
            greekLetters.Add("THETA");
            greekLetters.Add("IOTA");
            greekLetters.Add("KAPPA");
            greekLetters.Add("LAMBDA");
            greekLetters.Add("MU");
            greekLetters.Add("NU");
            greekLetters.Add("XI");
            greekLetters.Add("OMICRON");
            greekLetters.Add("PI");
            greekLetters.Add("RHO");
            greekLetters.Add("SIGMA");
            greekLetters.Add("TAU");
            greekLetters.Add("UPSILON");
            greekLetters.Add("PHI");
            greekLetters.Add("CHI");
            greekLetters.Add("PSI");
            greekLetters.Add("OMEGA");

            greekLetters = greekLetters.Shuffle().Shuffle().ToList();

            RandomGreekLetter = new Stack<string>();

            foreach (var greekLetter in greekLetters)
            {
                RandomGreekLetter.Push(greekLetter);
            }
        }

        public static string DamagedScannerUnit()
        {
            const int places = 3;
            Double totalpossibilities = Math.Pow(2, places);
            double exitNumber = Utility.Random.Next(Convert.ToInt32(totalpossibilities));

            var nebulaPattern = Convert.ToString(Convert.ToInt32(exitNumber), 2).PadLeft(places, '0').Replace('0', '-').Replace('1', '+');

            return nebulaPattern;
        }

        public static string AdjustIfNebula(Region thisRegion, string direction, ref string shipSectorX, ref string shipSectorY)
        {
            if (thisRegion.Type == RegionType.Nebulae)
            {
                direction = "Unknown, due to interference";
            }

            var result = Utility.HideXorYIfNebula(thisRegion, shipSectorX, shipSectorY);

            shipSectorX = result.X;
            shipSectorY = result.Y;

            return direction;
        }

        public static OutputCoordinate HideXorYIfNebula(Region thisRegion, string x, string y)
        {
            if (thisRegion.Type == RegionType.Nebulae)
            {
                switch (StarTrek_KG.Utility.Utility.Random.Next(2))
                {
                    case 0:
                        x = "?";
                        break;
                    case 1:
                        y = "?";
                        break;
                    default:
                        x = "??";
                        y = "??";
                        break;
                }
            }

            return new OutputCoordinate(x, y);
        }
    }
}