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
                                                    "dlrs = Scan All Regions in Galaxy",
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

        public Debug(Ship shipConnectedTo, IGame game): base(shipConnectedTo)
        {
            this.Type = SubsystemType.Debug; //this is required if you want this system to be able to be looked up
            this.Damage = 0;
        }

        public override List<string> Controls(string command)
        {
            this.Prompt.Output.Queue.Clear();

            switch (command.ToLower())
            {
                case "dsrec":
                    //this.PrintGalacticRecord(this.Map.Regions); 
                    this.Prompt.Line("full galactic record with ship position as colored text, baddies as red");
                    this.Prompt.Line("Not Implemented Yet");
                    break;

                case "dsnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    this.Prompt.Line("Nav Command prompt, then outputs visual of NAV Track in an SRS window");
                    this.Prompt.Line("Not Implemented Yet");
                    break;

                case "dstor":
                    //Torpedoes.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    this.Prompt.Line("Torpedo Command prompt, then outputs visual of Torpedo Track in an SRS window");
                    this.Prompt.Line("Not Implemented Yet");
                    break;

                case "dqnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //this.PrintGalacticRecord(WithNavTrack); 
                    this.Prompt.Line("Nav Command prompt, then outputs visual of NAV Track in a Galactic Map window");
                    this.Prompt.Line("Not Implemented Yet");
                    break;

                case "dibd":

                    //todo: newly appeared ship needs to NOT fire inbetween turns!

                    var testShipNames = this.ShipConnectedTo.Game.Config.FactionShips(FactionName.TestFaction).ToList().Shuffle();

                    var RegionX = Coordinate.GetRandom();
                    var RegionY = Coordinate.GetRandom();

                    var randomSector = new Sector(new LocationDef(RegionX, RegionY));

                    this.ShipConnectedTo.Game.Map.Config = this.ShipConnectedTo.Game.Config;

                    var hostileShip = new Ship(FactionName.Klingon, testShipNames[0], randomSector, this.ShipConnectedTo.Game.Map, this.ShipConnectedTo.Game);
                    Shields.For(hostileShip).Energy = Utility.Utility.Random.Next(100, 200); //todo: resource those numbers out

                    this.ShipConnectedTo.Game.Map.Regions.GetActive().AddShip(hostileShip, hostileShip.Sector);

                    //todo: if there not enough names set up for opposing ships things could break, or ships will have duplicate names
                    this.Prompt.Line(
                        $"Hostile Ship: \"{hostileShip.Name}\" just warped into sector [{randomSector.X},{randomSector.Y}]");

                    this.Prompt.Line(
                        $"Scanners indicate {hostileShip.Name}'s Energy: {hostileShip.Energy} Shields: {Shields.For(hostileShip).Energy} ");
                    break;

                case "disb":
 
                    //var RegionX = Coordinate.GetRandom();
                    //var RegionY = Coordinate.GetRandom();

                    //var randomSector = new Sector(new LocationDef(RegionX, RegionY));

                    //this.Game.Map.Config = this.Game.Config;

                    //var starbase = new Starbase("starbaseAlpha", this.Game.Map, new Sector(new LocationDef(new Coordinate(0, 0), new Coordinate(2, 7))));

                    var activeRegion = this.ShipConnectedTo.Game.Map.Regions.GetActive();

                    //activeRegion[randomSector].Item = SectorItem.Starbase;
                    //activeRegion.AddShip(starbase, starbase.Sector);

                    //var hostileShip = new Ship(testShipNames[0], randomSector, this.Game.Map);

                    //this.Game.Map.Regions.GetActive().AddShip(hostileShip, hostileShip.Sector);

                    ////todo: if there not enough names set up for opposing ships things could break, or ships will have duplicate names
                    //this.Game.Write.Line("Hostile Ship: \"" + hostileShip.Name + "\" just warped into sector [" + randomSector.X + "," + randomSector.Y + "]");
                    //this.Game.Write.Line("Scanners indicate " + hostileShip.Name + "'s Energy: " + hostileShip.Energy + " Shields: " + Shields.For(hostileShip).Energy + " ");
                    break;
                case "dist":
                    var sectorWithNewStar = this.ShipConnectedTo.Game.Map.Regions.GetActive().AddStar(this.ShipConnectedTo.Game.Map.Regions.GetActive());
                    this.Prompt.Line("A star has just formed spontaneously at: " +
                                $"[{sectorWithNewStar.X},{sectorWithNewStar.Y}]");
                    this.Prompt.Line($"Stellar Cartography has named it: {((Star) sectorWithNewStar.Object).Name}");
                    break;

                case "dbgm":
                    DEFAULTS.DEBUG_MODE = !DEFAULTS.DEBUG_MODE;
                    this.Prompt.Line($"Debug Mode set to: {DEFAULTS.DEBUG_MODE}.  This will clear on app restart.");
                    break;

                case "dlrs":
                    LongRangeScan.For(this.ShipConnectedTo).Debug_Scan_All_Regions(DEFAULTS.DEBUG_MODE);
                    this.Prompt.Line( $"All Regions set to: {DEFAULTS.DEBUG_MODE}.  (set debugmode to true to make this scan all.)");
                    break;

                default:
                    this.Prompt.Line(">> exiting Debug Mode..");
                    break;
            }

            return this.Prompt.Output.Queue.ToList();
        }

        public static Debug For(IShip ship)
        {
            return (Debug)For(ship, SubsystemType.Debug);
        }
    }
}
