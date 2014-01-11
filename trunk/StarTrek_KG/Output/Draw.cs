using System;
using System.Collections.Generic;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Output
{
    public class Draw: ICommand, IWrite
    {
        public Command Command { get; set; }
        public Write Write { get; set; }   
  
        public Draw(Write write, Command command)
        {
            this.Write = write;
            this.Command = command;

            if (this.Write == null)
            {
                throw new GameException("Property Write is not set for Draw ");
            }

            if (this.Command == null)
            {
                throw new GameException("Property Command is not set for Draw ");
            }
        }

        public void RandomAppTitle()
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

            this.Write.Resource("AppTitleSpace");

            RandomPicture();

            this.Write.Resource("AppTitleSpace");
        }

        private void RandomPicture()
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

        private void AppTitleItem(string itemName, int endingLine)
        {
            for (int i = 1; i < endingLine; i++)
            {
                this.Write.Resource("AppTitle" + itemName + i);
            }
        }

        public string Course()
        {
            return Environment.NewLine +
                   " 4   5   6 " + Environment.NewLine +
                   @"   \ ↑ /  " + Environment.NewLine +
                   "3 ← <*> → 7" + Environment.NewLine +
                   @"   / ↓ \  " + Environment.NewLine +
                   " 2   1   8" + Environment.NewLine +
                   Environment.NewLine;
        }

        public void Panel(string panelHead, IEnumerable<string> strings)
        {
            this.Command.Console.WriteLine();
            this.Command.Console.WriteLine(panelHead);
            this.Command.Console.WriteLine();

            foreach (var str in strings)
            {
                this.Command.Console.WriteLine(str);
            }

            this.Command.Console.WriteLine();
        }

        public void RenderQuadrantCounts(bool renderingMyLocation, int starbaseCount, int starCount, int hostileCount)
        {
            if (renderingMyLocation)
            {
                Write.HighlightTextBW(true);
            }

            this.Write.WithNoEndCR(hostileCount.FormatForLRS());
            this.Write.WithNoEndCR(starbaseCount.FormatForLRS());
            this.Write.WithNoEndCR(starCount.FormatForLRS());

            if (renderingMyLocation)
            {
                Write.HighlightTextBW(false);
            }
        }
    }
}
