using System;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Subsystem
{
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
                                                    "(These features might actually get included in the game when complete)",
                                                    "  ",
                                                    "dgrec = Galactic Record With Ship's position",
                                                    "dsnav = visual of NAV Track in an SRS window",
                                                    "dstor = Torpedo Track in an SRS window", //if this becomes a feature, then baddie needs to be replaced by explosion.  find a cool Unicode Character for it..
                                                    "dqnav = NAV Track in a Galactic Map window" 
                                                };

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
            var starship = this.Map.Playership;

            switch (command.ToLower())
            {
                case "dsrec":
                    //Output.PrintGalacticRecord(this.Map.Quadrants); 
                    Output.Write.Line("full galactic record with ship position as colored text, baddies as red");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dsnav":
                    //Navigation.For(this.Map.Playership).Controls(this.Map);
                    //ShortRangeScan.For(this.Map.Playership).Controls(this.Map);
                    Output.Write.Line("Nav Command prompt, then outputs visual of NAV Track in an SRS window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dstor":
                    //Torpedoes.For(this.Map.Playership).Controls(this.Map);
                    //ShortRangeScan.For(this.Map.Playership).Controls(this.Map);
                    Output.Write.Line("Torpedo Command prompt, then outputs visual of Torpedo Track in an SRS window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                case "dqnav":
                    //Navigation.For(this.Map.Playership).Controls(this.Map);
                    //Output.PrintGalacticRecord(WithNavTrack); 
                    Output.Write.Line("Nav Command prompt, then outputs visual of NAV Track in a Galactic Map window");
                    Output.Write.Line("Not Implemented Yet");
                    break;

                default:
                    //Output.WriteLine("Invalid debug command.");
                    break;
            }
        }

        public new static Computer For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (Debug). Check config file "); 
            }

            return (Computer)ship.Subsystems.Single(s => s.Type == SubsystemType.Computer);
        }
    }
}
