using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;
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
        public static readonly string[] CONTROL_PANEL = {
                                                    " ",
                                                    "--- > Debug/New Feature Test Mode ------------------------------------",
                                                    "  ",
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

        public Debug(Map map, Ship shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Map = map;
            this.Type = SubsystemType.Debug; //this is required if you want this system to be able to be looked up
            this.Damage = 0;
        }

        public override void OutputDamagedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputRepairedMessage()
        {
            throw new NotImplementedException();
        }

        public override void OutputMalfunctioningMessage()
        {
            throw new NotImplementedException();
        }

        public override void Controls(string command)
        {
            switch (command.ToLower())
            {
                case "dsrec":
                    //Output.PrintGalacticRecord(this.Map.Quadrants); 
                    Output.Write.Line("full galactic record with ship position as colored text, baddies as red");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dsnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    Output.Write.Line("Nav Command prompt, then outputs visual of NAV Track in an SRS window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dstor":
                    //Torpedoes.For(this.ShipConnectedTo).Controls(this.Map);
                    //ShortRangeScan.For(this.ShipConnectedTo).Controls(this.Map);
                    Output.Write.Line("Torpedo Command prompt, then outputs visual of Torpedo Track in an SRS window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dqnav":
                    //Navigation.For(this.ShipConnectedTo).Controls(this.Map);
                    //Output.PrintGalacticRecord(WithNavTrack); 
                    Output.Write.Line("Nav Command prompt, then outputs visual of NAV Track in a Galactic Map window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dibd":

                    //todo: newly appeared ship needs to NOT fire inbetween turns!

                    var testShipNames = StarTrekKGSettings.GetShips("TestFaction").ToList().Shuffle();

                    var quadX = Coordinate.GetRandom();
                    var quadY = Coordinate.GetRandom();

                    var randomSector = new Sector(new LocationDef(quadX, quadY));

                    var hostileShip = new Ship(testShipNames[0], this.Map, randomSector);

                    this.Map.Quadrants.GetActive().AddShip(hostileShip, hostileShip.Sector);

                    //todo: if there not enough names set up for opposing ships things could break, or ships will have duplicate names
                    Output.Write.Line("Hostile Ship: \"" + hostileShip.Name + "\" just warped into sector [" + randomSector.X + "," + randomSector.Y + "]");
                    Output.Write.Line("Scanners indicate " + hostileShip.Name + "'s Energy: " + hostileShip.Energy + " Shields: " + hostileShip.Shields().Energy + " ");
                    break;

                case "dist":
                    var sectorWithNewStar = Map.AddStar(this.Map.Quadrants.GetActive());
                    Output.Write.Line("A star has just formed spontaneously at: " + "[" + sectorWithNewStar.X + "," + sectorWithNewStar.Y + "]");
                    Output.Write.Line("Stellar Cartography has named it: " + ((Star)sectorWithNewStar.Object).Name);
                    break;

                default:
                    Output.Write.Line("Invalid debug command.");
                    break;
            }
        }

        public new static Debug For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Debug). Check config file "); 
            }

            return (Debug)ship.Subsystems.Single(s => s.Type == SubsystemType.Debug);
        }
    }
}
