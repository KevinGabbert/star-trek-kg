using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;

namespace StarTrek_KG.Actors
{

    //TODO: ship.Energy not decrementing after being hit
    public class Ship : ISystem, IShip
    {
        //todo: needs access to quadrants and utility and game for subsystem FOR mnemonic to work for DI
        #region Properties

            //todo: (maybe) create function GetQuadrant() to replace this (will query map.quadrants for ship)
            public Coordinate Coordinate { get; set; }

            //todo: create function GetSector() to replace this (will query map.quadrants.active for ship)
            public ISector Sector { get; set; } //This is a ship's location in a sector
            public Allegiance Allegiance { get; set; }
            public Subsystems Subsystems { get; set; }
            public Type Type { get; set; }
            public IMap Map { get; set; }
            public IStarTrekKGSettings Config { get; set; }

            public string Name { get; set; }
            public FactionName Faction { get; set; }
            public double Energy { get; set; }
            public bool Destroyed { get; set; }

            ////todo: status of the battles will be kept in the ships LOG.  If you board a ship, you can read its log and see who it had a battle with.
            //public Log Log { get; set; } //

            //todo: get current quadrant of ship so list of baddies can be kept.
        #endregion

        public Ship(FactionName faction, string name, ISector sector, IConfig map)
        {
            this.Map = (IMap)CheckParam(map);

            if (faction == null)
            {
                throw new GameException("null faction for Ship Creation.  *Everyone* has a Faction!");
            }

            if (this.Map.Quadrants == null)
            {
                throw new GameException("Map not set up with quadrants");
            }

            if (this.Map.Config == null)
            {
                throw new GameException("Map not set up with Config");
            }
            
            this.Config = (IStarTrekKGSettings)CheckParam(map.Config);

            if (sector.QuadrantDef == null)
            {
                throw new GameConfigException("Ship has no sector.QuadrantDef set up.");
            }
            else
            {
                this.Coordinate = sector.QuadrantDef;
                this.Sector = (ISector)CheckParam(sector);
            }

            this.Type = this.GetType();
            this.Allegiance = this.GetAllegiance(); 
            this.Name = name;
            this.Faction = faction;
            
            this.Subsystems = new Subsystems(this.Map, this, this.Config);

            //todo: support the shieldEnergy config setting.
            //If there is a config setting, use it.  otherwise, 0

            //todo: pull config settings here
            //refactor from Game.GetGlobalInfo()
        }

        private static object CheckParam(object prop)
        {
            if (prop == null)
            {
                throw new GameException("New Ship param is null");
            }
            return prop;
        }

        public void RepairEverything()
        {
            this.Energy = this.Config.GetSetting<int>("repairEnergy");

            this.Subsystems.FullRepair();
        }

        public Allegiance GetAllegiance()
        {
            var setting = this.Config.GetSetting<string>("Hostile");

            Allegiance returnVal;

            switch (setting)
            {
                case "BadGuy":
                    returnVal = Allegiance.BadGuy;
                    break;
                case "GoodGuy":
                    returnVal = Allegiance.GoodGuy;
                    break;
                default:
                    returnVal = Allegiance.Indeterminate;
                    break;
            }

            return returnVal;
        }

        public void Scavenge(ScavengeType scavengeType)
        {
            int photonsScavenged = 0;
            int energyScavenged = 0;
            string scavengedFrom = "";

            switch (scavengeType)
            {
                case ScavengeType.Starbase:
                    photonsScavenged = Utility.Utility.Random.Next(10); //todo: resource out this number
                    energyScavenged = Utility.Utility.Random.Next(2500); //todo: resource out this number
                    scavengedFrom = "Starbase";
                    break;

                case ScavengeType.FederationShip:
                    photonsScavenged = Utility.Utility.Random.Next(5); //todo: resource out this number
                    energyScavenged = Utility.Utility.Random.Next(750); //todo: resource out this number
                    scavengedFrom = "Federation starship";
                    break;

                case ScavengeType.OtherShip:
                    photonsScavenged = Utility.Utility.Random.Next(2); //todo: resource out this number
                    energyScavenged = Utility.Utility.Random.Next(100); //todo: resource out this number
                    scavengedFrom = "ship";
                    break;
            }

            //seem like a lot? well.. you are taking on the ENTIRE FEDERATION!  You will need it!

            var scavengedText = "";
            var foundPhotons = (photonsScavenged > 0);

            if (foundPhotons)
            {
                Torpedoes.For(this).Count += photonsScavenged;
                scavengedText = photonsScavenged + " Torpedoes found";
            }

            if (energyScavenged > 0)
            {
                this.Energy += energyScavenged;

                if (foundPhotons)
                {
                    scavengedText += ", & "; 
                }

                scavengedText += energyScavenged + " Energy found";
            }

            this.Map.Write.Line(scavengedText + " from destroyed " + scavengedFrom + " debris field. ");
        }
          
        ///interesting..  one could take a hit from another map.. Wait for the multidimensional version of this game.  (now in 3D!) :D
        /// returns true if ship was destroyed. (hence, ship could not absorb all energy)
        public void AbsorbHitFrom(IShip attacker, int attackingEnergy) 
        {
            string hitMessage = Write.ShipHitMessage(attacker);

            this.Map.Write.Line(hitMessage);

            var shields = Shields.For(this);
            shields.Energy -= attackingEnergy;

            bool shieldsWorking = (this.GetQuadrant().Type != QuadrantType.Nebulae);

            if (!shieldsWorking)
            {
                this.Map.Write.Line("Shields ineffective due to interference from Nebula");
            }

            if ((shields.Energy < 0) || !shieldsWorking)
            {
                this.TakeDamageOrDestroyShip(attackingEnergy, shields);
            }
            else
            {
                this.Map.Write.Line("No Damage.");
            }
        }

        private void TakeDamageOrDestroyShip(int attackingEnergy, System shields)
        {
            //reclaim any energy back to the ship.  'cause we are nice like that.
            if (shields.Energy > 0)
            {
                this.Map.Write.Line("Reclaiming shield energy back to ship.");
                this.Energy += shields.Energy;
            }

            shields.Energy = 0; //for the benefit of the output message telling the user that they have no shields. )

            bool assignedDamage = this.Subsystems.TakeDamageIfWeCan(attackingEnergy);

            this.Map.Write.Line("Energy is now at: " + this.Energy);
            if (!assignedDamage)
            {
                //this means there was nothing left to damage.  Blow the ship up.
                this.Energy = 0;
                this.Destroyed = true;
            }
            else
            {
                //It hurts more when you have no shields.  This is a balance point
                this.Energy = this.Energy - (attackingEnergy * 3 ); //todo: resource this out
                    //todo: make this multiplier an app.config setting //todo: write a test to verify this behavior.

                if (this.Energy < 0)
                {
                    this.Destroyed = true;
                }
            }
        }

        //todo: create a GetLastQuadrant & GetLastSector
        public Quadrant GetQuadrant()
        {
            //todo: get rid of this.Map ?
            var retVal = this.Map.Quadrants.Where(s => s.X == this.Coordinate.X && s.Y == this.Coordinate.Y).ToList();

            if (retVal == null)
            {
                throw new GameConfigException("Quadrant X: " + this.Coordinate.X + " Y: " + this.Coordinate.Y + " not found.");
            }

            if (!retVal.Any())
            {
                if (this.Coordinate == null)
                {
                    throw new GameConfigException("Coordinate not found for ship. Ship has no location set up anywhere..");
                }
                else
                {
                    throw new GameConfigException("Quadrant X: " + this.Coordinate.X + " Y: " + this.Coordinate.Y + " not found.");
                }
            }

            return retVal.Single();
        }

        public Location GetLocation()
        {
            var shipLocation = new Location();
            shipLocation.Sector = this.Sector;
            shipLocation.Quadrant = this.GetQuadrant();

            return shipLocation;
        }

        public bool AtLowEnergyLevel()
        {
            return (this.Energy < this.Config.GetSetting<int>("LowEnergyLevel"));
        }

        public string GetConditionAndSetIcon()
        {
            var currentQuadrant = this.GetQuadrant();
            var condition = "GREEN";

            if (currentQuadrant.GetHostiles().Count > 0)
            {
                condition = "RED";

                ConsoleHelper.SetConsoleIcon(SystemIcons.Error);
            }
            else if (this.AtLowEnergyLevel() || currentQuadrant.IsNebulae())
            {
                condition = "YELLOW";
                ConsoleHelper.SetConsoleIcon(SystemIcons.Exclamation);
            }
            else
            {
                ConsoleHelper.SetConsoleIcon(SystemIcons.Shield);
            }

            return condition;
        }
    }
}
