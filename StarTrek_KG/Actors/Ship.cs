using System;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Actors
{
    //TODO: ship.Energy not decrementing after being hit
    public class Ship : ISystem, IShip
    {
        //todo: needs access to quadrants and utility and game for subsystem FOR mnemonic to work for DI
        #region Properties

            //todo: (maybe) create function GetQuadrant() to replace this (will query map.quadrants for ship)
            public Coordinate QuadrantDef { get; set; }

            //todo: create function GetSector() to replace this (will query map.quadrants.active for ship)
            public ISector Sector { get; set; } //This is a ship's location in a sector
            public Allegiance Allegiance { get; set; }
            public Subsystems Subsystems { get; set; }
            public Type Type { get; set; }
            public Map Map { get; set; }
            public IStarTrekKGSettings Config { get; set; }

            public string Name { get; set; }
            public double Energy { get; set; }
            public bool Destroyed { get; set; }

            ////todo: status of the battles will be kept in the ships LOG.  If you board a ship, you can read its log and see who it had a battle with.
            //public Log Log { get; set; } //

            //todo: get current quadrant of ship so list of baddies can be kept.
        #endregion

        public Ship(string name, ISector position, Map map, IStarTrekKGSettings config)
        {
            this.Map = (Map)CheckParam(map);
            this.Config = (IStarTrekKGSettings)CheckParam(config);
            this.Sector = (ISector)CheckParam(position);

            this.Type = this.GetType();
            this.Allegiance = this.GetAllegiance(); 
            this.Name = name;
            this.QuadrantDef = position.QuadrantDef;
            
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
            else
            {
                return prop;
            }
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
          
        ///interesting..  one could take a hit from another map.. Wait for the multidimensional version of this game.  (now in 3D!) :D
        /// returns true if ship was destroyed. (hence, ship could not absorb all energy)
        public void AbsorbHitFrom(IShip attacker, int attackingEnergy) //
        {
            this.Map.Write.Line(string.Format(this.Name + " hit by " + attacker.Name + " at sector [{0},{1}].... ", (attacker.Sector.X), (attacker.Sector.Y)));

            var shields = Shields.For(this);
            shields.Energy -= attackingEnergy;

            if (shields.Energy < 0)
            {
                this.TakeDamageOrDestroyShip(attackingEnergy, shields);
            }
            else
            {
                this.Map.Write.Line("No Structural Damage from hit.");
            }
        }

        private void TakeDamageOrDestroyShip(int attackingEnergy, System shields)
        {
            shields.Energy = 0; //for the benefit of the output message telling the user that they have no shields. )

            bool assignedDamage = this.Subsystems.TakeDamageIfWeCan(attackingEnergy);
            if (!assignedDamage)
            {
                //this means there was nothing left to damage.  Blow the ship up.
                this.Energy = 0;
                this.Destroyed = true;
            }
            else
            {
                //It hurts more when you have no shields.  This is a balance point
                this.Energy = this.Energy - (attackingEnergy *3 ); //todo: resource this out
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
            var retVal = this.Map.Quadrants.Where(s => s.X == this.QuadrantDef.X && s.Y == this.QuadrantDef.Y).ToList();

            if (retVal == null)
            {
                throw new GameConfigException("Quadrant X: " + this.QuadrantDef.X + " Y: " + this.QuadrantDef.Y + " not found.");
            }

            if (!retVal.Any())
            {
                throw new GameConfigException("Quadrant X: " + this.QuadrantDef.X + " Y: " + this.QuadrantDef.Y + " not found.");
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
    }
}
