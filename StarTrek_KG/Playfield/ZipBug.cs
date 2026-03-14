using System;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    public class ZipBug : ICoordinateObject
    {
        private static readonly string[] Aliases =
        {
            "FFFFBKT",
            "FFTTHXLT",
            "FFFRLX-A",
            "FFFFNFT-#%$!",
            "FFFFRFC",
            "FFFLSITT",
            "FFFFIIPBG",
            "FFFFFPFFF",
            "Deluge",
            "FFFOOooo",
            "FFFFIVBG",
            "FFTTHXLT Masked",
            "FFFFFRANK",
            "FFFFHBBTT",
            "FFFFSCHP",
            "TBFZZT",
            "FFPHHBBTT",
            "ZZZFFTTG",
            "ZZZFFTG",
            "ZZZFFFTT",
            "Eroded Signal",
            "IPFITT",
            "Renewal",
            "Unappreciated Necessity",
            "FAHZT",
            "FHPPTF",
            "FFFFLSHIT",
            "FFFFRRRR",
            "FFFFANG",
            "FFFFFFKKK",
            "FXXXZTZT",
            "Market Crash",
            "Market Top",
            "FFFFLXI",
            "FFHTIPP",
            "Skittering Illumination",
            "FFFFSCHP",
            "FFFFHBBXX",
            "Revamped",
            "FFFFBXT",
            "Watermarked",
            "FFFFNFT-*>%#",
            "FFFFBX",
            "Crypto Twitter",
            "FFFFEep",
            "FFZILAA-A",
            "FFFFRING",
            "FFFFRRRRR",
            "Degraded",
            "IPFITT",
            "FFFFFFRRKK",
            "Protean Changeling",
            "FFFFOOO!",
            "FFPHHBBT",
            "Lantern with a Timer",
            "Lotused Lamp",
            "FFFFIIBG",
            "FFBBBBBBBT",
            "FFPHHBBTT A Fury Fiend",
            "Hieroglyph",
            "FFFFSTMP",
            "Misunderstood Contraption",
            "FFFFSSHKRT",
            "FFFFOOOO!",
            "Crowned",
            "FFBBBT",
            "FFFFHIPTA",
            "FFFFIPBG",
            "Otherworldly Abomination",
            "Harold",
            "FFPHHBBTT Waterboarded",
            "Reinvention",
            "Refit",
            "FFFFZKRK",
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
            "Elite",
            "FFFFZTZZ",
            "ZZZZFFTT",
            "ZZZZFTT",
            "FFFFHBB",
            "FFFRLX",
            "FFFFOOO!",
            "FFFFFTMP",
            "FFFFWMMPP",
            "FFRXXOZ",
            "FFPHHBBTT Recycled",
            "FFPHPTP Exhausted",
            "FFPHHBBTT Refit of Days",
            "FFTHXLT Rewired",
            "FFTHXLT Realized",
            "FFTHXLT Generated",
            "FFTHXLT Aged Future",
            "FFTHXLT Remastered",
            "FFTHXLT Transmogrified",
            "FFFFOOO!",
            "FFFFRANKKK",
            "FFFFTSMP",
            "FFFFCHP"
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
