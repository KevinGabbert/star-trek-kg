using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class ZipBug : ICoordinateObject
    {
        private static readonly string[] Aliases =
        {
            "Deluge",
            "Eroded Signal",
            "Renewal",
            "Unappreciated Necessity",
            "Market Crash",
            "Market Top",
            "Skittering Illumination",
            "Revamped",
            "Watermarked",
            "Crypto Twitter",
            "Degraded",
            "Protean Changeling",
            "Lantern with a Timer",
            "Lotused Lamp",
            "Hieroglyph",
            "Misunderstood Contraption",
            "Crowned",
            "Otherworldly Abomination",
            "Harold",
            "Reinvention",
            "Refit",
            "Revamped",
            "Regeneration",
            "Post Nuclear",
            "Flagrance",
            "Illuminated Bulwark",
            "Illusion of Valuation",
            "Fiery the angels fell.",
            "Prosthetic ZedNah",
            "Waterboarded",
            "Cloud Edifice",
            "Lantern Unit",
            "Bugsplom",
            "Chromatic",
            "Mechanized",
            "Fixelated",
            "Exhumed",
            "Elite"
        };

        public enum ZipBugForm
        {
            HostileMimic,
            FigureEight,
            StarMimic
        }

        public ICoordinate Coordinate { get; set; }
        public Type Type { get; set; }
        public string Name { get; set; }
        public ZipBugForm Form { get; set; }
        public int TurnsInCurrentSector { get; set; }
        public bool WasAdjacentToPlayerLastTurn { get; set; }

        public ZipBug()
        {
            this.Type = typeof(ZipBug);
            this.Name = Aliases[Utility.Utility.Random.Next(Aliases.Length)];
            this.Form = (ZipBugForm)Utility.Utility.Random.Next(3);
        }

        public void ResetForRelocation()
        {
            this.Name = Aliases[Utility.Utility.Random.Next(Aliases.Length)];
            this.Form = (ZipBugForm)Utility.Utility.Random.Next(3);
            this.TurnsInCurrentSector = 0;
            this.WasAdjacentToPlayerLastTurn = false;
        }
    }
}
