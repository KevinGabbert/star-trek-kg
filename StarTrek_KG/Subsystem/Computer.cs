using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    //todo: make feature where opposing ships can hack into your computer (and you, theirs) if shields are down
    public class Computer : SubSystem_Base
    {
        public static readonly string[] CONTROL_PANEL = {
                                                    "",
                                                    "─── Main Computer ──────────────",
                                                    "rec = Cumulative Galactic Record",
                                                    "sta = Status Report",
                                                    "tor = Photon Torpedo Calculator",
                                                    "bas = Starbase Calculator",
                                                    "nav = Navigation Calculator",
                                                    "tlm = Translate Last Message"
                                                };

        public Computer(Ship shipConnectedTo, Game game): base(shipConnectedTo, game)
        {
            this.Type = SubsystemType.Computer;
            this.Damage = 0;
        }

        public override List<string> Controls(string command)
        {
            this.Game.Write.Output.OutputQueue.Clear();

            var starship = this.ShipConnectedTo;

            switch (command.ToLower())
            {
                case "rec":
                    Computer.For(this.ShipConnectedTo).PrintGalacticRecord(this.Game.Map.Regions);
                    break;

                case "sta":

                    //todo: get a list of all baddie names in Region

                    this.PrintCurrentStatus(this.Game.Map, 
                                              this.Damage, 
                                              starship,
                                              this.ShipConnectedTo.GetRegion());
                    break;

                case "tor":
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Torpedoes.For(this.ShipConnectedTo).Calculator();
                    break;

                case "toq":
                    this.TargetObjectInRegion();
                    break;

                case "bas":
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Navigation.For(this.ShipConnectedTo).StarbaseCalculator(this.ShipConnectedTo); 
                    break;

                case "nav":
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Navigation.For(this.ShipConnectedTo).Calculator();
                    break;

                case "tlm":

                    if (this.Game.LatestTaunts != null && this.Game.LatestTaunts.Count > 0)
                    {
                        TranslateLatestTaunt();
                    }
                    else
                    {
                        this.Game.Write.Line("Nothing to translate..");
                    }

                    break;

                default:
                    this.Game.Write.Line("Invalid computer this.Write");
                    break;
            }

            return this.Game.Write.Output.OutputQueue.ToList();
        }

        private void TargetObjectInRegion()
        {
            string replyFromUser;

            this.Game.Write.PromptUser("Target with (T)orpedoes or (P)hasers? ", out replyFromUser);

            switch (replyFromUser.ToUpper())
            {
                case "T":
                    Torpedoes.For(this.ShipConnectedTo).TargetObject();
                    break;

                case "P":
                    Phasers.For(this.ShipConnectedTo).TargetObject();
                    break;
            }
        }

        public List<KeyValuePair<int, Sector>> ListObjectsInRegion()
        {
            var list = new List<KeyValuePair<int, Sector>>();

            if (this.Damaged())
            {
                Game.Write.Line("Unable to List Objects in Region");
                return list;
            }

            var sectorsWithObjects = ShortRangeScan.For(this.ShipConnectedTo).ObjectFinder().ToList();

            if (sectorsWithObjects.Any())
            {
                int objectNumber = 1;
                foreach (var sector in sectorsWithObjects)
                {
                    string objectName = sector.Object != null ? sector.Object.Name : "Unknown";

                    this.Game.Write.SingleLine(string.Format(objectNumber + ": {0}  [{1},{2}].", objectName, (sector.X + 1),
                        (sector.Y + 1)));

                    list.Add(new KeyValuePair<int, Sector>(objectNumber, sector));

                    objectNumber++;
                }
            }
            else
            {
                this.Game.Write.Line("No Sectors with Objects found (this is an error)");
            }

            return list;
        }

        private void TranslateLatestTaunt()
        {
            this.Game.Write.Line("Comms was able to translate the latest transmissions: ");

            foreach (FactionThreat taunt in this.Game.LatestTaunts)
            {
                this.Game.Write.Line(taunt.Translation == ""
                    ? "No Translation required."
                    : taunt.Translation);
            }
        }

        //output this as KeyValueCollection that the UI can display as it likes.

        public void PrintCurrentStatus(IMap map, int computerDamage, Ship ship, Region currentRegion)
        {
            if (this.Damaged()) return;

            //todo: completely redo this

            this.Game.Write.Console.WriteLine("");
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSTimeRemaining"), map.timeRemaining);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSHostilesRemaining"), map.Regions.GetHostileCount());
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSHostilesInRegion"), currentRegion.GetHostiles().Count);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSStarbases"), map.starbases);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSWarpEngineDamage"), Navigation.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSSRSDamage"), ShortRangeScan.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSLRSDamage"), LongRangeScan.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSCRSDamage"), CombinedRangeScan.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSShieldsDamage"), Shields.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSComputerDamage"), computerDamage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSPhotonDamage"), Torpedoes.For(ship).Damage);
            this.Game.Write.Console.WriteLine(this.Game.Config.GetText("CSPhaserDamage"), Phasers.For(ship).Damage);
            this.Game.Write.Console.WriteLine();

            //foreach (var badGuy in currentRegion.Hostiles)
            //{
            //    
            //}

            this.Game.Write.Console.WriteLine();

            //todo: Display all baddie names in Region when encountered.
        }

        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app

        public void PrintGalacticRecord(List<Region> Regions)
        {
            if (this.Damaged()) return;

            this.Game.Write.Console.WriteLine();
            this.Game.Write.ResourceSingleLine("GalacticRecordLine");

            var myLocation = this.ShipConnectedTo.GetLocation();

            for (var RegionLB = 0; RegionLB < Constants.Region_MAX; RegionLB++)
            {
                for (var RegionUB = 0; RegionUB < Constants.Region_MAX; RegionUB++)
                {
                    //todo: refactor this function
                    //todo: this needs to be refactored with LRS!

                    this.Game.Write.WithNoEndCR(Constants.SCAN_SECTOR_DIVIDER);

                    var Region = Playfield.Regions.Get(Regions, new Coordinate(RegionUB, RegionLB));
                    if (Region.Scanned)
                    {
                        this.RenderScannedRegion(Region, myLocation, RegionUB, RegionLB);
                    }
                    else
                    {
                        this.Game.Write.RenderUnscannedRegion(myLocation.Region.X == RegionUB && myLocation.Region.Y == RegionLB); //renderingMyLocation todo: refactor with other calls for that.
                    }
                }

                this.Game.Write.SingleLine(Constants.SCAN_SECTOR_DIVIDER);
                this.Game.Write.ResourceSingleLine("GalacticRecordLine");
            }

            this.Game.Write.Console.WriteLine();
        }

        private void RenderScannedRegion(IRegion Region, Location myLocation, int RegionUB, int RegionLB)
        {
            int starbaseCount = -1;
            int starCount = -1;
            int hostileCount = -1;

            if (Region.Type != RegionType.Nebulae)
            {
                starbaseCount = Region.GetStarbaseCount();
                starCount = Region.GetStarCount();
                hostileCount = Region.GetHostiles().Count();
            }

            bool renderingMyLocation = false;

            if (myLocation.Region.Scanned)
            {
                renderingMyLocation = myLocation.Region.X == RegionUB && myLocation.Region.Y == RegionLB;
            }

            if (Region.Type != RegionType.Nebulae)
            {
                this.Game.Write.RenderRegionCounts(renderingMyLocation, starbaseCount, starCount, hostileCount);
            }
            else
            {
                this.Game.Write.RenderNebula(renderingMyLocation);
            }
        }

        public static Computer For(IShip ship)
        {
            return (Computer)SubSystem_Base.For(ship, SubsystemType.Computer);
        }
    }
}
