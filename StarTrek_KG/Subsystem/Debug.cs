using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Settings;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;

namespace StarTrek_KG.Subsystem
{
    //todo: investigate removing this base class
    public class Debug : SubSystem_Base //temporary, until this area gets rewritten
    {
        //todo: debug should not really be a subsystem.  Debug mode should be outside of the subsystem pattern, 
        //but I wanted to slap it together in a hurry.  That would be hilarious if DebugMode can get damaged if hit by a baddie (I haven't tested that but I guess its possible!  :D)

        //todo:  its actually not a bad idea to integrate these tracks into the standard game visuals itself..  Hmmmmmm.
        //todo: Getting tired of seeing that Torp. track show up as an array  For now these visuals are accessed here..

        //todo: change to List<string>

        //todo: resource out this menu
        public static readonly string[] DEBUG_PANEL = {
                                                    " ",
                                                    "--- > Debug/New Feature Test Mode ------------------------------------",
                                                    "  ",
                                                    "dbgm = Toggle Debug Mode",
                                                    "dlrs = Scan All Sectors in Galaxy",
                                                    "drec = Galactic Record With Ship's position",
                                                    "dnav = visual of NAV Track in an SRS window",
                                                    "dtor = Torpedo Track in an SRS window", //if this becomes a feature, then baddie needs to be replaced by explosion.  find a cool Unicode Character for it..
                                                    "dnvt = NAV Track in a Galactic Map window",
                                                    "----- = -------------------------------------",
                                                    "dibd = Insert non-firing baddie into current quardrant",
                                                    "dist = Insert star into current quardrant",
                                                    "difb = Insert firing baddie into current quardrant",
                                                    "disb = Insert starbase into current quardrant",
                                                    "dpph = Playership takes a random energy level phaser hit",
                                                    "dpth = Playership takes a torpedo hit",
                                                    "dpdh = Playership takes a random energy level disruptor hit",
                                                    "dmbb = move baddie around sector",
                                                    "dadd = add energy to Playership",
                                                    "dads = add shield energy to ship" //it should be: dadd  Who? (then user selects a number from a list of ships) How much?
                                                };

        public Debug(IShip shipConnectedTo): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Debug; //this is required if you want this system to be able to be looked up
            this.Damage = 0;
        }

        public override List<string> Controls(string command)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();

            switch (command.ToLower())
            {
                case "dsrec":
                    //this.PrintGalacticRecord(this.Map.Sectors); 
                    this.ShipConnectedTo.OutputLine("full galactic record with ship position as colored text, baddies as red");
                    this.ShipConnectedTo.OutputLine("Not Implemented Yet");
                    break;

                case "dsnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    this.ShipConnectedTo.OutputLine("Nav Command prompt, then outputs visual of NAV Track in an SRS window");
                    this.ShipConnectedTo.OutputLine("Not Implemented Yet");
                    break;

                case "dstor":
                    //Torpedoes.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    this.ShipConnectedTo.OutputLine("Torpedo Command prompt, then outputs visual of Torpedo Track in an SRS window");
                    this.ShipConnectedTo.OutputLine("Not Implemented Yet");
                    break;

                case "dqnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //this.PrintGalacticRecord(WithNavTrack); 
                    this.ShipConnectedTo.OutputLine("Nav Command prompt, then outputs visual of NAV Track in a Galactic Map window");
                    this.ShipConnectedTo.OutputLine("Not Implemented Yet");
                    break;

                case "dibd":

                    //todo: newly appeared ship needs to NOT fire inbetween turns!

                    var testShipNames = this.ShipConnectedTo.Map.Game.Config.ShipNames(FactionName.TestFaction).ToList().Shuffle();

                    var RegionX = Point.GetRandom();
                    var RegionY = Point.GetRandom();

                    var randomSector = new Coordinate(new LocationDef(RegionX, RegionY));

                    this.ShipConnectedTo.Map.Config = this.ShipConnectedTo.Map.Game.Config;

                    var hostileShip = new Ship(FactionName.Klingon, testShipNames[0], randomSector, this.ShipConnectedTo.Map);
                    Shields.For(hostileShip).Energy = Utility.Utility.Random.Next(100, 200); //todo: resource those numbers out

                    this.ShipConnectedTo.Map.Sectors.GetActive().AddShip(hostileShip, hostileShip.Coordinate);

                    //todo: if there not enough names set up for opposing ships things could break, or ships will have duplicate names
                    this.ShipConnectedTo.OutputLine(
                        $"Hostile Ship: \"{hostileShip.Name}\" just warped into sector [{randomSector.X},{randomSector.Y}]");

                    this.ShipConnectedTo.OutputLine(
                        $"Scanners indicate {hostileShip.Name}'s Energy: {hostileShip.Energy} Shields: {Shields.For(hostileShip).Energy} ");
                    break;

                case "disb":
 
                    //var RegionX = Point.GetRandom();
                    //var RegionY = Point.GetRandom();

                    //var randomSector = new Coordinate(new LocationDef(RegionX, RegionY));

                    //this.Game.Map.Config = this.Game.Config;

                    //var starbase = new Starbase("starbaseAlpha", this.Game.Map, new Coordinate(new LocationDef(new Point(0, 0), new Point(2, 7))));

                    var activeRegion = this.ShipConnectedTo.Map.Sectors.GetActive();

                    //activeRegion[randomSector].Item = CoordinateItem.Starbase;
                    //activeRegion.AddShip(starbase, starbase.Coordinate);

                    //var hostileShip = new Ship(testShipNames[0], randomSector, this.Game.Map);

                    //this.Game.Map.Sectors.GetActive().AddShip(hostileShip, hostileShip.Coordinate);

                    ////todo: if there not enough names set up for opposing ships things could break, or ships will have duplicate names
                    //this.Game.Write.Line("Hostile Ship: \"" + hostileShip.Name + "\" just warped into sector [" + randomSector.X + "," + randomSector.Y + "]");
                    //this.Game.Write.Line("Scanners indicate " + hostileShip.Name + "'s Energy: " + hostileShip.Energy + " Shields: " + Shields.For(hostileShip).Energy + " ");
                    break;
                case "dist":
                    var sectorWithNewStar = this.ShipConnectedTo.Map.Sectors.GetActive().AddStar(this.ShipConnectedTo.Map.Sectors.GetActive());
                    this.ShipConnectedTo.OutputLine("A star has just formed spontaneously at: " +
                                $"[{sectorWithNewStar.X},{sectorWithNewStar.Y}]");
                    this.ShipConnectedTo.OutputLine($"Stellar Cartography has named it: {((Star) sectorWithNewStar.Object).Name}");
                    break;

                case "dbgm":
                    DEFAULTS.DEBUG_MODE = !DEFAULTS.DEBUG_MODE;
                    this.ShipConnectedTo.OutputLine($"Debug Mode set to: {DEFAULTS.DEBUG_MODE}.  This will clear on app restart.");
                    break;

                case "dlrs":
                    LongRangeScan.For(this.ShipConnectedTo).Debug_Scan_All_Regions(DEFAULTS.DEBUG_MODE);
                    this.ShipConnectedTo.OutputLine( $"All Sectors set to: {DEFAULTS.DEBUG_MODE}.  (set debugmode to true to make this scan all.)");
                    break;

                default:
                    this.ShipConnectedTo.OutputLine(">> exiting Debug Mode..");
                    break;
            }

            return this.ShipConnectedTo.OutputQueue();
        }

        public new static Debug For(IShip ship)
        {
            return (Debug)For(ship, SubsystemType.Debug);
        }
    }
}
