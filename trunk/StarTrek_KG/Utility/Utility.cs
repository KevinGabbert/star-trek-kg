using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StarTrek_KG.Config;

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

        //calculates distance traveled mostly to be able to have a value for diagonal distance, as straight line calculation doesnt need all this effort.
        //todo: modify this to take location
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
        public static double ShootBeamWeapon(double energyToPowerWeapon, double distance, string deprecationRateConfigKey, string energyAdjustmentConfigKey)
        {
            double deprecationRate = (new StarTrekKGSettings()).GetSetting<double>(deprecationRateConfigKey);
            double energyAdjustment = (new StarTrekKGSettings()).GetSetting<double>(energyAdjustmentConfigKey);

            double deliveredEnergy = StarTrek_KG.Utility.Utility.ComputeBeamWeaponIntensity(energyToPowerWeapon, energyAdjustment, distance, deprecationRate);

            return deliveredEnergy;
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

        public static string NebulaUnit()
        {
            const int places = 3;
            Double totalpossibilities = Math.Pow(2, places);
            double exitNumber = Utility.Random.Next(Convert.ToInt32(totalpossibilities));

            var nebulaPattern = Convert.ToString(Convert.ToInt32(exitNumber), 2).PadLeft(places, '0').Replace('0', '-').Replace('1', '+');

            return nebulaPattern;
        }
    }


    //Todo: when this works, move this to its own file.
    public class ObjectWalkerEntity
    {
        public object Value { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

    public static class ObjectWalker
    {
        public static List<ObjectWalkerEntity> Walk(object o)
        {
            return ProcessObject(o).ToList();
        }

        private static IEnumerable<ObjectWalkerEntity> ProcessObject(object o)
        {
            if (o == null)
            {
                // nothing here, just return an empty enumerable object
                return new ObjectWalkerEntity[0];
            }

            // create the list to hold values found in this object
            var objectList = new List<ObjectWalkerEntity>();

            Type t = o.GetType();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (IsGeneric(pi.PropertyType))
                {
                    // Add generic object
                    var obj = new ObjectWalkerEntity();
                    obj.PropertyInfo = pi;
                    object value;
                    try
                    {
                        value = pi.GetValue(o, null);

                        objectList.Add(obj);
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }

                }
                else
                {
                    // not generic, get the property value and make the recursive call
                    try
                    {
                        object value = pi.GetValue(o, null);

                        // all values returned from the recursive call get 
                        // rolled up into the list created in this call.
                        objectList.AddRange(ProcessObject(value));
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }


                }
            }

            return objectList.AsReadOnly();
        }

        private static bool IsGeneric(Type type)
        {
            return
                IsSubclassOfRawGeneric(type, typeof(bool)) ||
                IsSubclassOfRawGeneric(type, typeof(string)) ||
                IsSubclassOfRawGeneric(type, typeof(int)) ||
                IsSubclassOfRawGeneric(type, typeof(UInt16)) ||
                IsSubclassOfRawGeneric(type, typeof(UInt32)) ||
                IsSubclassOfRawGeneric(type, typeof(UInt64)) ||
                IsSubclassOfRawGeneric(type, typeof(DateTime));
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}



//ALPHA	        α	&#945;	&#x03B1	&alpha;
//BETA	        β	&#946;	&#x03B2	&beta;
//GAMMA	        γ	&#947;	&#x03B3	&gamma;
//DELTA	        δ	&#948;	&#x03B4	&delta;
//EPSILON	    ε	&#949;	&#x03B5	&epsilon;
//ZETA	        ζ	&#950;	&#x03B6	&zeta;
//ETA	        η	&#951;	&#x03B7	&eta;
//THETA	        θ	&#952;	&#x03B8	&theta;
//IOTA	        ι	&#953;	&#x03B9	&iota;
//KAPPA	        κ	&#954;	&#x03BA	&kappa;
//LAMBDA	    λ	&#955;	&#x03BB	&lambda;
//MU	        μ	&#956;	&#x03BC	&mu;
//NU	        ν	&#957;	&#x03BD	&nu;
//XI	        ξ	&#958;	&#x03BE	&xi;
//OMICRON	    ο	&#959;	&#x03BF	&omicron;
//PI	        π	&#960;	&#x03C0	&pi;
//RHO	        ρ	&#961;	&#x03C1	&rho;
//FINAL SIGMA	ς	&#962;	&#x03C2	 
//SIGMA	        σ	&#963;	&#x03C3	&sigma;
//TAU	        τ	&#964;	&#x03C4	&tau;
//UPSILON	    υ	&#965;	&#x03C5	&upsilon;
//PHI	        φ	&#966;	&#x03C6	&phi;
//CHI	        χ	&#967;	&#x03C7	&chi;
//PSI	        ψ	&#968;	&#x03C8	&psi;
//OMEGA	        ω	&#969;	&#x03C9	&omega;

//αβγδεζηθικλμνξοπρςστυφχψω