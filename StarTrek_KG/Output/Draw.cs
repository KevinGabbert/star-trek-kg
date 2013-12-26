using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    Draw.AppTitleItem("Classic", 7);
                    break;

                case 2:
                    Draw.AppTitleItem("TNG", 7);
                    break;

                default:
                    Draw.AppTitleItem("Movie", 7);
                    break;
            }

            Write.Resource("AppTitleSpace");

            Draw.RandomPicture();

            Write.Resource("AppTitleSpace");
        }

        private static void RandomPicture()
        {
            Utility.Utility.Random = new Random(Guid.NewGuid().GetHashCode());
            int randomVal = Utility.Utility.Random.Next(120);
            switch (randomVal)
            {
                case 1:
                    Draw.AppTitleItem("ExcelsiorSmall", 8);
                    break;

                case 2:
                    Draw.AppTitleItem("DaedalusSmall", 8);
                    break;

                case 3:
                    Draw.AppTitleItem("Reliant", 8);
                    break;

                case 4:
                    Draw.AppTitleItem("D7Front", 6);
                    break;

                case 5:
                    Draw.AppTitleItem("KTingaSide", 9);
                    break;

                case 6:
                    Draw.AppTitleItem("DreadnaughtSide", 9);
                    break;

                case 7:
                    Draw.AppTitleItem("Excelsior", 9);
                    break;

                case 8:
                    Draw.AppTitleItem("EnterpriseB", 10);
                    break;

                case 9:
                    Draw.AppTitleItem("EnterpriseD", 9);
                    break;

                case 11:
                    Draw.AppTitleItem("BattlecruiserSmall", 6);
                    break;

                default:
                    Draw.AppTitleItem("2ShipsSmall", 7);
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
    }
}
