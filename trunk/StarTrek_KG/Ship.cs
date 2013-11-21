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

            public List<ISubsystem> Subsystems { get; set; }

            //todo: get current quadrant of ship so list of baddies can be kept.
        #endregion

        public Ship(string name, Map map, Sector position)
        {
            this.Map = map;
            this.Sector = new Sector(new LocationDef(null, new Coordinate(position.X, position.Y)));
            this.Allegiance = this.GetAllegiance(); 
            this.Name = name;
            this.QuadrantDef = position.QuadrantDef;
            
            this.Subsystems = new List<ISubsystem>();

            //todo: support the shieldEnergy config setting.
            //If there is a config setting, use it.  otherwise, 0

            this.Subsystems = new List<ISubsystem>()
                                  {
                                     new Shields(map) { Energy = 0 },
                                     new Computer(map),
                                     new Navigation(map),
                                     new ShortRangeScan(map),
                                     new LongRangeScan(map),
                                     new Torpedoes(map),
                                     new Phasers(map)
                                  };

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
        public bool RepairSubsystem(Ship ship)
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
        public static bool AbsorbHitFrom(Ship attacker, Map map)
        {
            var ship = map.Playership.GetLocation();
            var distance = Map.Distance(ship.Sector.X, 
                                        ship.Sector.Y,
                                        attacker.Sector.X,
                                        attacker.Sector.Y);

            Shields.For(map.Playership).Energy -= map.DisruptorShot(300, 11.3, distance); //todo: pull values from config

            Ship.UpdateDestroyedStatus(map);

            Console.WriteLine(map.Playership.Name + " hit by " + attacker.Name + " at sector [{0},{1}]. Shields dropped to {2}.",
                             (attacker.Sector.X), (attacker.Sector.Y), Shields.For(map.Playership).Energy);

            return Shields.For(map.Playership).Energy != 0;
        }

        private static void UpdateDestroyedStatus(Map map)
        {
            if (Shields.For(map.Playership).Energy < 0)
            {
                Shields.For(map.Playership).Energy = 0; //for the benefit of the output message telling the user that they have no shields. )
                map.Playership.Energy = 0;
                map.Playership.Destroyed = true;
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
    }
}
