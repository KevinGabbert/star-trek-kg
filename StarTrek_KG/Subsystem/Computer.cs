using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Constants.Commands;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;
using StarTrek_KG.TypeSafeEnums;
using static StarTrek_KG.Subsystem.SubSystem_Base;

namespace StarTrek_KG.Subsystem
{
    //todo: make feature where opposing ships can hack into your computer (and you, theirs) if shields are down
    public class Computer : SubSystem_Base
    {
        //todo: resource this out menu
        public static readonly string[] CONTROL_PANEL = {
                                                            "",
                                                            "─── Main Computer ──────────────",
                                                            "rec = Cumulative Galactic Record",
                                                            "sta = Status Report",
                                                            "tor = Photon Torpedo Calculator",
                                                            "bas = Starbase Calculator",
                                                            "nav = Navigation Calculator",
                                                            "tlm = Translate Last Message" //todo: new feature, auto translate upgrade
                                                        };

        public Computer(Ship shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Computer;
            this.Damage = 0;
        }

        public override List<string> Controls(string command)
        {
            IGame game = this.ShipConnectedTo.Map.Game;

            game.Interact.Output.Queue.Clear();

            var starship = this.ShipConnectedTo;

            //todo: thise commands should be pulled from web.config entries
            switch (command.ToLower())
            {
                case Commands.Computer.GalacticRecord:
                    Computer.For(this.ShipConnectedTo).PrintGalacticRecord(game.Map.Regions);
                    break;

                case Commands.Computer.Status:

                    //todo: get a list of all baddie names in Region

                    this.PrintCurrentStatus(game.Map, 
                                              this.Damage, 
                                              starship,
                                              this.ShipConnectedTo.GetRegion());
                    break;

                case Commands.Computer.TorpedoCalculator:
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Torpedoes.For(this.ShipConnectedTo).Calculator();
                    break;

                case Commands.Computer.TargetObjectInRegion:
                    this.TargetObjectInRegion();
                    break;

                case Commands.Computer.StarbaseCalculator:
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Navigation.For(this.ShipConnectedTo).StarbaseCalculator(this.ShipConnectedTo); 
                    break;

                case Commands.Computer.NavigationCalculator:
                    //todo: calculator code will be refactored to this object
                    if (this.Damaged()) return null;
                    Navigation.For(this.ShipConnectedTo).Calculator();
                    break;

                case Commands.Computer.TranslateLastMessage:

                    if (game.LatestTaunts != null && game.LatestTaunts.Count > 0)
                    {
                        this.TranslateLatestTaunt();
                    }
                    else
                    {
                        game.Interact.Line("Nothing to translate..");
                    }

                    break;

                default:
                    game.Interact.Line("Invalid computer this.Write");
                    break;
            }

            return game.Interact.Output.Queue.ToList();
        }

        private void TargetObjectInRegion()
        {
            string replyFromUser;

            this.ShipConnectedTo.Map.Game.Interact.PromptUserConsole("Target with (T)orpedoes or (P)hasers? ", out replyFromUser);

            switch (replyFromUser.ToUpper())
            {
                case "T": //todo: enum this
                    Torpedoes.For(this.ShipConnectedTo).TargetObject();
                    break;

                case "P": //todo: enum this
                    Phasers.For(this.ShipConnectedTo).TargetObject();
                    break;
            }
        }

        public List<KeyValuePair<int, Sector>> ListObjectsInRegion()
        {
            var list = new List<KeyValuePair<int, Sector>>();

            if (this.Damaged())
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("Unable to List Objects in Region");
                return list;
            }

            var sectorsWithObjects = ShortRangeScan.For(this.ShipConnectedTo).ObjectFinder().ToList();

            if (sectorsWithObjects.Any())
            {
                int objectNumber = 1;
                foreach (var sector in sectorsWithObjects)
                {
                    string objectName = sector.Object != null ? sector.Object.Name : "Unknown";

                    this.ShipConnectedTo.Map.Game.Interact.SingleLine(string.Format(objectNumber + ": {0}  [{1},{2}].", objectName, (sector.X + 1),
                        (sector.Y + 1)));

                    list.Add(new KeyValuePair<int, Sector>(objectNumber, sector));

                    objectNumber++;
                }
            }
            else
            {
                this.ShipConnectedTo.Map.Game.Interact.Line("No Sectors with Objects found (this is an error)");
            }

            return list;
        }

        private void TranslateLatestTaunt()
        {
            this.Prompt.Line("Comms was able to translate the latest transmissions: ");

            foreach (FactionThreat taunt in this.ShipConnectedTo.Map.Game.LatestTaunts)
            {
                this.Prompt.Line(taunt.Translation == "" ? "No Translation required." : taunt.Translation);
            }
        }

        //output this as KeyValueCollection that the UI can display as it likes.

        private void PrintCurrentStatus(IMap map, int computerDamage, IShip ship, IRegion currentRegion)
        {
            if (this.Damaged()) return;

            IStarTrekKGSettings config = map.Game.Config;
            IOutputMethod output = map.Game.Interact.Output;

            //todo: completely redo this

            output.WriteLine("");
            output.WriteLine(config.GetText("CSTimeRemaining"), map.timeRemaining);
            output.WriteLine(config.GetText("CSHostilesRemaining"), map.Regions.GetHostileCount());
            output.WriteLine(config.GetText("CSHostilesInRegion"), currentRegion.GetHostiles().Count);
            output.WriteLine(config.GetText("CSStarbases"), map.starbases);
            output.WriteLine(config.GetText("CSWarpEngineDamage"), Navigation.For(ship).Damage);
            output.WriteLine(config.GetText("CSSRSDamage"), ShortRangeScan.For(ship).Damage);
            output.WriteLine(config.GetText("CSLRSDamage"), LongRangeScan.For(ship).Damage);
            output.WriteLine(config.GetText("CSCRSDamage"), CombinedRangeScan.For(ship).Damage);
            output.WriteLine(config.GetText("CSShieldsDamage"), Shields.For(ship).Damage);
            output.WriteLine(config.GetText("CSComputerDamage"), computerDamage);
            output.WriteLine(config.GetText("CSPhotonDamage"), Torpedoes.For(ship).Damage);
            output.WriteLine(config.GetText("CSPhaserDamage"), Phasers.For(ship).Damage);
            output.WriteLine();

            //foreach (var badGuy in currentRegion.Hostiles)
            //{
            //    
            //}

            output.WriteLine();

            //todo: Display all baddie names in Region when encountered.
        }

        //This needs to be output as an array, List<List>, or KeyValueCollection, and the grid needs to be generated by the UI app

        private void PrintGalacticRecord(IReadOnlyCollection<Region> Regions)
        {
            if (this.Damaged()) return;

            this.Prompt.Output.WriteLine();
            this.Prompt.ResourceSingleLine("GalacticRecordLine");

            var myLocation = this.ShipConnectedTo.GetLocation();

            for (var RegionLB = 0; RegionLB < DEFAULTS.REGION_MAX; RegionLB++)
            {
                for (var RegionUB = 0; RegionUB < DEFAULTS.REGION_MAX; RegionUB++)
                {
                    //todo: refactor this function
                    //todo: this needs to be refactored with LRS!

                    this.Prompt.WithNoEndCR(DEFAULTS.SCAN_SECTOR_DIVIDER);

                    var Region = Playfield.Regions.Get(Regions, new Coordinate(RegionUB, RegionLB));
                    if (Region.Scanned)
                    {
                        this.RenderScannedRegion(Region, myLocation, RegionUB, RegionLB);
                    }
                    else
                    {
                        this.Prompt.RenderUnscannedRegion(myLocation.Region.X == RegionUB && myLocation.Region.Y == RegionLB); //renderingMyLocation todo: refactor with other calls for that.
                    }
                }

                this.Prompt.SingleLine(DEFAULTS.SCAN_SECTOR_DIVIDER);
                this.Prompt.ResourceSingleLine("GalacticRecordLine");
            }

            this.Prompt.Output.WriteLine();
        }

        private void RenderScannedRegion(IRegion Region, Location myLocation, int RegionUB, int RegionLB)
        {
            int? starbaseCount = null;
            int? starCount = null;
            int? hostileCount = null;

            if (Region.Type != RegionType.Nebulae)
            {
                starbaseCount = Region.GetStarbaseCount();
                starCount = Region.GetStarCount();
                hostileCount = Region.GetHostiles().Count;
            }

            bool renderingMyLocation = false;

            if (myLocation.Region.Scanned)
            {
                renderingMyLocation = myLocation.Region.X == RegionUB && myLocation.Region.Y == RegionLB;
            }

            if (Region.Type != RegionType.Nebulae)
            {
                this.Prompt.RenderRegionCounts(renderingMyLocation, starbaseCount, starCount, hostileCount);
            }
            else
            {
                this.Prompt.RenderNebula(renderingMyLocation);
            }
        }

        public static Computer For(IShip ship)
        {
            return (Computer)For(ship, SubsystemType.Computer);
        }
    }
}
