using System;
using System.Linq;
using System.Collections.Generic;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG
{
    //TODO: ship.Energy not decrementing after being hit
    public class Ship : System, IShip
    {
        #region Properties

            //todo: (maybe) create function GetQuadrant() to replace this (will query map.quadrants for ship)
            private Coordinate _quadrantDef;
            public Coordinate QuadrantDef
            {
                get
                {
                    return _quadrantDef;
                }
                set
                {
                    _quadrantDef = value;

                    //todo: when setting up playership, this breaks!!  **************************
                    //fix in movement

                    //this.GetQuadrant().Active = true;
                }
            }

            //todo: create function GetSector() to replace this (will query map.quadrants.active for ship)
            public Sector Sector { get; set; } //This is a ship's location in a sector

            public string Name { get; set; }
            public Allegiance Allegiance { get; set; }

            public Subsystems Subsystems { get; set; }

            //todo: get current quadrant of ship so list of baddies can be kept.
        #endregion

        public Ship(string name, Map map, Sector position)
        {
            this.Map = map;
            this.Sector = new Sector(new LocationDef(null, new Coordinate(position.X, position.Y)));
            this.Allegiance = this.GetAllegiance(); 
            this.Name = name;
            this.QuadrantDef = position.QuadrantDef;
            
            this.Subsystems = new Subsystems(map);

            //todo: support the shieldEnergy config setting.
            //If there is a config setting, use it.  otherwise, 0

            //todo: pull config settings here
            //refactor from Game.GetGlobalInfo()
        }

        public void RepairEverything()
        {     
            this.Energy = 3000; //todo: pull from app.config "repairEnergy"

            //todo: starbases can upgrade energy maximums during game
            
            ShortRangeScan.For(this).Damage = 0;
            LongRangeScan.For(this).Damage = 0;
            Computer.For(this).Damage = 0;
            Phasers.For(this).Damage = 0;
            Shields.For(this).Energy = 0;

            var torpedoes = Torpedoes.For(this);
            torpedoes.Count = 10;
            torpedoes.Damage = 0;

            var starship = Navigation.For(this);
            starship.docked = true;
            starship.Damage = 0;
        }

        /// <summary>
        /// repairs one item every time called
        /// </summary>
        /// <returns></returns>
        public bool RepairSubsystems(Ship ship)
        {
            //TODO: make the priority level configurable
            return ShortRangeScan.For(ship).Repair() ||
                   LongRangeScan.For(ship).Repair() ||
                   Navigation.For(ship).Repair() ||
                   Computer.For(ship).Repair() ||
                   Shields.For(ship).Repair() ||
                   Torpedoes.For(ship).Repair() ||
                   Phasers.For(ship).Repair();
        }

        public Allegiance GetAllegiance()
        {
            //todo: remove this app setting (pass in as an argument?)
            var setting = AppConfig.Setting<string>("Hostile");

            return setting == "Bad Guy" ? Allegiance.GoodGuy : Allegiance.BadGuy;
        }
          
        ///interesting.  one could take a hit from another map.. Wait for the multidimensional version of this game.  (now in 3D!) :D
        /// returns true if ship was destroyed. (hence, ship could not absorb all energy)
        public bool AbsorbHitFrom(Ship attacker, Map map)
        {
            var ship = map.Playership.GetLocation();
            var distance = Utility.Distance(ship.Sector.X, 
                                        ship.Sector.Y,
                                        attacker.Sector.X,
                                        attacker.Sector.Y);

            //TODO: Currently, ships can only be struck by Disruptor.  Modify so ship can take a hit from a photon
            //This could be as simple as setting an energy level for a photon, and renaming DisruptorShot to be something else..
            Shields.For(map.Playership).Energy -= Ship.DisruptorShot(distance); //todo: pull values from config

            this.UpdateShipHealthStatus(map);

            Console.WriteLine(map.Playership.Name + " hit by " + attacker.Name + " at sector [{0},{1}]. Shields dropped to {2}.",
                             (attacker.Sector.X), (attacker.Sector.Y), Shields.For(map.Playership).Energy);

            return Shields.For(map.Playership).Energy != 0;
        }

        private void UpdateShipHealthStatus(Map map)
        {
            if (Shields.For(this).Energy < 0)
            {
                int boltStrength = Math.Abs(Shields.For(this).Energy);

                //todo: if(Math.Abs(Shields.For(this).Energy) == all subsystems damage remaining, then blow up the ship.
                //This is going to require each subsystem to have a damage number, which has to be implemented.
                //if there are 5 subsystems, and each subsystem has 50 damage points left, then a bolt of 2400 should
                //either #1. knock out 4 of them and one almost all the way, or #2. leave 5 barely functioning systems.
                //#1 is easier to code, #2 would be more interesting to code, and would allow for responses to different
                //weapons types.  Perhaps an Ion Cannon can take out only shields and weapons and warp, but leave everything
                //else intact.  or another kind of bolt could hit all systems the same, taking some out, and leaving others
                //barely functioning.

                //TODO: for the moment, this is our current behavior.  The opposing ship might not want to unload all phaser power into an enemy, as it will be wasted
                Shields.For(this).Energy = 0; //for the benefit of the output message telling the user that they have no shields. )

                bool tookDamage = this.Subsystems.TakeDamageIfAppropriate(boltStrength);
                if (!tookDamage)
                {
                    //this means there was nothing left to damage.  Blow the ship up.
                    map.Playership.Energy = 0;
                    map.Playership.Destroyed = true;
                }
            }   
        }

        //todo: create a GetLastQuadrant & GetLastSector
        public Quadrant GetQuadrant()
        {
            var retVal = this.Map.Quadrants.Where(s => s.X == this.QuadrantDef.X && s.Y == this.QuadrantDef.Y).ToList();

            if(retVal == null)
            {
                throw new GameConfigException("Quadrant X: " + this.QuadrantDef.X + " Y: " + this.QuadrantDef.Y + " not found.");
            }

            if (retVal.Count() == 0)
            {
                throw new GameConfigException("Quadrant X: " + this.QuadrantDef.X + " Y: " + this.QuadrantDef.Y + " not found.");
            }

            return retVal.Single();
        }

        public Location GetLocation()
        {
            var shipLocation = new Location();
            shipLocation.Sector = this.Map.Playership.Sector;
            shipLocation.Quadrant = this.Map.Playership.GetQuadrant();

            return shipLocation;
        }


        /// <summary>
        /// This function represents the amount of energy fired by an opposing ship.
        /// The value is a seeded random number that decreases by distance.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static int DisruptorShot(double distance)
        {
            //todo: give ship a disruptor weapon type, enable it only on hostileType.Klingon.  delete this.

            var seed = AppConfig.Setting<int>("DisruptorShotSeed"); //todo: pull from config
            var distanceDeprecationLevel = AppConfig.Setting<double>("DisruptorShotDeprecationLevel"); //todo: pull deprecationlevel from config

            var adjustedDisruptorEnergy = (AppConfig.Setting<double>("DisruptorEnergyAdjustment") - distance / distanceDeprecationLevel);
            var deliveredEnergy = (int)(seed * (Utility.Random).NextDouble() * adjustedDisruptorEnergy);

            return deliveredEnergy;
        }
    }
}
