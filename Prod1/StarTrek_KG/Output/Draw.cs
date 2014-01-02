using System;
using System.Collections.Generic;

namespace StarTrek_KG.Output
{
    public class Draw
    {
        public static void RandomAppTitle()
        {
            int randomVal = Utility.Utility.Random.Next(3);

            switch (randomVal)
            {
                case 0:
                    AppTitleItem("Classic", 7);
                    break;

                case 2:
                    AppTitleItem("TNG", 7);
                    break;

                default:
                    AppTitleItem("Movie", 7);
                    break;
            }

            Write.Resource("AppTitleSpace");

            RandomPicture();

            Write.Resource("AppTitleSpace");
        }

        private static void RandomPicture()
        {
            Utility.Utility.Random = new Random(Guid.NewGuid().GetHashCode());
            int randomVal = Utility.Utility.Random.Next(150);
            switch (randomVal)
            {
                case 1:
                    AppTitleItem("ExcelsiorMedium", 8);
                    break;

                case 2:
                    AppTitleItem("DaedalusSmall", 8);
                    break;

                case 3:
                    AppTitleItem("Reliant", 8);
                    break;

                case 4:
                    AppTitleItem("D7Front", 6);
                    break;

                case 5:
                    AppTitleItem("D-10-", 6);
                    break;

                case 6:
                    AppTitleItem("D-4-", 7);
                    break;

                case 7:
                    AppTitleItem("D-11-", 6);
                    break;

                case 8:
                    AppTitleItem("D-18-", 6);
                    break;

                case 9:
                    AppTitleItem("D-27-", 7);
                    break;

                case 10:
                    AppTitleItem("AkulaSmall", 7);
                    break;

                case 11:
                    AppTitleItem("BattlecruiserSmall", 6);
                    break;

                case 12:
                    AppTitleItem("SaladinSmall", 6);
                    break;

                case 13:
                    AppTitleItem("EagleSmall", 6);
                    break;

                case 14:
                    AppTitleItem("DreadnaughtSide", 9);
                    break;

                case 15:
                    AppTitleItem("Enterprise-BSmall", 6);
                    break;

                case 16:
                    AppTitleItem("ExcelsiorSmall", 6);
                    break;

                case 17:
                    AppTitleItem("RomulanBOP", 7);
                    break;

                default:
                    AppTitleItem("2ShipsSmall", 7);
                    break;
            }
        }

        private static void AppTitleItem(string itemName, int endingLine)
        {
            for (int i = 1; i < endingLine; i++)
            {
                Write.Resource("AppTitle" + itemName + i);
            }
        }

        public static string Course()
        {
            return Environment.NewLine +
                   " 4   5   6 " + Environment.NewLine +
                   @"   \ ↑ /  " + Environment.NewLine +
                   "3 ← <*> → 7" + Environment.NewLine +
                   @"   / ↓ \  " + Environment.NewLine +
                   " 2   1   8" + Environment.NewLine +
                   Environment.NewLine;
        }

        public static void Panel(string panelHead, IEnumerable<string> strings)
        {
            Command.Console.WriteLine();
            Command.Console.WriteLine(panelHead);
            Command.Console.WriteLine();

            foreach (var str in strings)
            {
                Command.Console.WriteLine(str);
            }

            Command.Console.WriteLine();
        }
    }
}
