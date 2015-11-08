using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;
using StarTrek_KG.Utility;
using Region = StarTrek_KG.Playfield.Region;

namespace StarTrek_KG.Actors
{
    //TODO: ship.Energy not decrementing after being hit
    public class Ship : ISystem, IShip
    {
        //todo: needs access to Regions and utility and game for subsystem FOR mnemonic to work for DI
        #region Properties

            public Game Game { get; set; }

            //todo: (maybe) create function GetRegion() to replace this (will query map.Regions for ship)
            public Coordinate Coordinate { get; set; }

            //todo: create function GetSector() to replace this (will query map.Regions.active for ship)
            public ISector Sector { get; set; } //This is a ship's location in a Region
            public Allegiance Allegiance { get; set; }
            public Subsystems Subsystems { get; set; }
            public Type Type { get; set; }
            public IMap Map { get; set; }
            public IStarTrekKGSettings Config { get; set; }

            public string Name { get; set; }
            public FactionName Faction { get; set; }
            public int Energy { get; set; }
            public bool Destroyed { get; set; }

            ////todo: status of the battles will be kept in the ships LOG.  If you board a ship, you can read its log and see who it had a battle with.
            //public Log ShipLog { get; set; } //

            //todo: get current Region of ship so list of baddies can be kept.
        #endregion

        public Ship(FactionName faction, string name, ISector sector, IConfig map, Game game)
        {
            this.Game = game;

            this.Map = (IMap)CheckParam(map);

            //todo: this could actually be a feature later..
            if (faction == null)
            {
                throw new GameException(this.Config.GetText("NullFaction"));
            }

            if (this.Map.Regions == null)
            {
                throw new GameException(this.Config.GetText("MapHasNoRegions"));
            }

            if (this.Map.Config == null)
            {
                throw new GameException(this.Config.GetText("MapHasNoConfig"));
            }
            
            this.Config = (IStarTrekKGSettings)CheckParam(map.Config);

            if (sector.RegionDef == null)
            {
                throw new GameConfigException(this.Config.GetText("ShipHasNoSector"));
            }
            else
            {
                this.Coordinate = sector.RegionDef;
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

        public override string ToString()
        {
            return $"Name: {this.Name} " +
                   $"Allegiance: {this.Allegiance} " +
                   $"Faction: {this.Faction} " +
                   $"Coordinate: {this.Coordinate} " +
                   $"Destroyed: {this.Destroyed} " +
                   $"Config: {this.Config} " +
                   $"Sector: {this.Sector} " +
                   $"Type: {this.Type} " +
                   $"SubSystems: {this.Subsystems} " +
                   $"Energy: {this.Energy} ";
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

        private Allegiance GetAllegiance()
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

            //todo: if found a ship with destroyed = true and it is not removed from map, but has subsystems with energy, then pull energy from all those subsystems

            switch (scavengeType)
            {
                case ScavengeType.Starbase:
                    photonsScavenged = Utility.Utility.Random.Next(10); //todo: resource out this number
                    energyScavenged = Utility.Utility.Random.Next(2500); //todo: resource out this number
                    scavengedFrom = this.Map.Config.GetText("StarbaseScavengeType");
                    break;

                case ScavengeType.FederationShip:
                    photonsScavenged = Utility.Utility.Random.Next(5); //todo: resource out this number
                    energyScavenged = Utility.Utility.Random.Next(750); //todo: resource out this number
                    scavengedFrom = this.Map.Config.GetText("FederationShipScavengeType");
                    break;

                case ScavengeType.OtherShip:

                    //var testingTorpsScavenged = Torpedoes.For(this).Game.RandomFactorForTesting;
                    //var testingEnergyScavenged = this.Game.RandomFactorForTesting;
                    
                    //int randomPhotonsScavenged = testingTorpsScavenged == 0 ? Utility.Utility.Random.Next(2) : testingTorpsScavenged;
                    //int randomEnergyScavenged = testingEnergyScavenged == 0 ? Utility.Utility.Random.Next(100) : testingEnergyScavenged;

                    photonsScavenged = Utility.Utility.TestableRandom(this.Game, 2, 2); //randomPhotonsScavenged; //todo: resource out this number
                    energyScavenged = Utility.Utility.TestableRandom(this.Game, 100, 100); //randomEnergyScavenged; //todo: resource out this number
                    scavengedFrom = this.Map.Config.GetText("OtherShipScavengeType"); ;
                    break;

                case ScavengeType.Star:
                    //todo: implement this later
                    scavengedFrom = this.Map.Config.GetText("StarScavengeType");
                    break;

                case ScavengeType.Asteroid:
                    //todo: implement this later
                    scavengedFrom = this.Map.Config.GetText("AsteroidScavengeType");
                    break;
            }

            //seem like a lot? well.. you are taking on the ENTIRE FEDERATION!  You will need it!

            var scavengedText = "";
            var foundPhotons = (photonsScavenged > 0);

            if (foundPhotons)
            {
                Torpedoes.For(this).Count += photonsScavenged;
                scavengedText = this.Map.Write.GetFormattedConfigText("PhotonsScavenged", photonsScavenged);
            }

            if (energyScavenged > 0)
            {
                this.Energy += energyScavenged;

                if (foundPhotons)
                {
                    scavengedText += ", & ";
                }

                scavengedText += this.Map.Write.GetFormattedConfigText("EnergyScavenged", energyScavenged);
            }

            this.Map.Write.FormattedConfigLine("ScavengedText", scavengedText, scavengedFrom);

            //todo: credit scavenged energy to ship energy.
            //note: create ship.autoscavenge, and turn it off for testing.

            this.UpdateDivinedSectors();
        }

        ///interesting..  one could take a hit from another map.. Wait for the multidimensional version of this game.  (now in 3D!) :D
        /// returns true if ship was destroyed. (hence, ship could not absorb all energy)
        public void AbsorbHitFrom(IShip attacker, int attackingEnergy)
        {
            var shields = Shields.For(this);

            if (attackingEnergy > 0)
            {
                string hitMessage = this.Game.Interact.ShipHitMessage(attacker, attackingEnergy);
                this.Map.Write.Line(hitMessage);

                shields.Energy -= attackingEnergy;
            }
            else
            {
                string message = this.Game.Interact.MisfireMessage(attacker);
                this.Map.Write.Line(message);
            }

            bool shieldsWorking = (this.GetRegion().Type != RegionType.Nebulae);

            if (!shieldsWorking)
            {
                this.Map.Write.ConfigText("ShieldsIneffectiveCauseNebula");
            }

            if ((shields.Energy < 0) || !shieldsWorking)
            {
                this.TakeDamageOrDestroyShip(attackingEnergy, shields);
            }
            else
            {
                this.Map.Write.ConfigText("NoDamage");
            }
        }

        private void TakeDamageOrDestroyShip(int attackingEnergy, System shields)
        {
            //reclaim any energy back to the ship.  'cause we are nice like that.
            if (shields.Energy > 0)
            {
                this.Map.Write.ConfigText("ReclaimingShieldEnergy");
                this.Energy += shields.Energy;
            }

            shields.Energy = 0; //for the benefit of the output message telling the user that they have no shields. )

            bool assignedDamage = this.Subsystems.TakeDamageIfWeCan(attackingEnergy);

            this.Map.Write.FormattedConfigLine("CurrentEnergyState", this.Energy);

            if (this.Energy <= 0)
            {
                this.Destroyed = true;
            }

            if (!this.Destroyed)
            {
                if (!assignedDamage)
                {
                    //this means there was nothing left to damage.  Blow the ship up.
                    this.Energy = 0;
                    this.Destroyed = true;
                }
                else
                {
                    if (this.Energy > attackingEnergy)
                    {
                        //It hurts more when you have no shields.  This is a balance point
                        this.Energy -= attackingEnergy; //*3 //todo: resource this out
                        //todo: make this multiplier an app.config setting //todo: write a test to verify this behavior. 
                    }

                    if (this.Energy < 0)
                    {
                        this.Destroyed = true;
                    }
                }
            }
        }

        //todo: create a GetLastRegion & GetLastSector
        public Playfield.Region GetRegion()
        {
            //todo: get rid of this.Map ?
            Region retVal = this.Map.Regions.SingleOrDefault(s => s.X == this.Coordinate.X && s.Y == this.Coordinate.Y);

            if (retVal == null)
            {
                if (this.Coordinate == null)
                {
                    throw new GameConfigException(this.Map.Config.GetText("CoordinateNotFound"));
                }
                else
                {
                    string regionNotFound = this.Map.Write.GetFormattedConfigText("RegionNotFound", this.Coordinate.X, this.Coordinate.Y);
                    throw new GameConfigException(regionNotFound);
                }
            }

            return retVal;
        }

        //todo: refactor these 2 into prop?
        public Location GetLocation()
        {
            var shipLocation = new Location
            {
                Sector = this.Sector, Region = this.GetRegion()
            };

            return shipLocation;
        }

        public bool AtLowEnergyLevel()
        {
            return (this.Energy < this.Config.GetSetting<int>("LowEnergyLevel"));
        }

        public string GetConditionAndSetIcon()
        {
            var currentRegion = this.GetRegion();
            var condition = this.Map.Config.GetText("ConditionNormal");

            if (currentRegion.GetHostiles().Count > 0)
            {
                condition = this.Map.Config.GetText("ConditionAlert");

                ConsoleHelper.SetConsoleIcon(SystemIcons.Error);
            }
            else if (this.AtLowEnergyLevel() || currentRegion.IsNebulae())
            {
                condition = this.Map.Config.GetText("ConditionElevated");
                ConsoleHelper.SetConsoleIcon(SystemIcons.Exclamation);
            }
            else
            {
                ConsoleHelper.SetConsoleIcon(SystemIcons.Shield);
            }

            return condition;
        }

        /// <summary>
        /// Used to get an idea of the area immediately surrounding the ship
        /// This was created primarily to determine if a sector surrounding the ship is in another Region for navigation purposes
        /// This function is called when the ship is set in place on the map.
        /// 
        /// Called at:
        /// - The end of Map Initialization
        /// - End of Navigation
        /// - When destroyed Ships are cleaned up
        /// </summary>
        /// <returns></returns>
        public void UpdateDivinedSectors()
        {
            //Scan happens like this, in number order:
            //036           //036                
            //147           //1X7  <-- X is you 
            //258           //258      

            //Immediate Range Scan

            //Busted
            //↑|↑|→   //+|-|+
            //E|P|→   //+|+|+
            //E|E|→   //-|-|-

            //Longhand
            //[3,6] E [3,7] S [3,→] FreeHold
            //[4,6] E [4,7] P [4,→] FreeHold
            //[5,6] E [5,7] E [5,→] FreeHold

            //←	↑	→	↓	
            //Portal to other Galaxy - ⇄
            //⇶

            //http://en.wikipedia.org/wiki/Template:Unicode_chart_Arrows

            var myLocation = this.GetLocation();

            this.Sector.Neighbors = new List<DivinedSectorItem>();

            int row = 0;

            for (var sectorL = myLocation.Sector.Y - 1; sectorL <= myLocation.Sector.Y + 1; sectorL++)
            {
                if (sectorL >= -1 && sectorL <= 8)
                {
                    for (var sectorT = myLocation.Sector.X - 1; sectorT <= myLocation.Sector.X + 1; sectorT++)
                    {
                        var currentResult = new DivinedSectorItem
                        {
                            MyLocation = myLocation.Sector.X == sectorT && myLocation.Sector.Y == sectorL, Location = new Location()
                        };

                        if (sectorT >= -1 && sectorT <= 8)
                        {
                            Sectors sectorsToQuery = myLocation.Region.Sectors;

                            currentResult.Location.Sector = sectorsToQuery.GetNoError(new Coordinate(sectorT, sectorL));

                            bool nullSector = currentResult.Location.Sector == null;

                            if (nullSector)
                            {
                                var currentRegion = myLocation.Region;

                                //todo: debug: fix this.
                                if (sectorT == 8 || sectorT == -1)
                                {
                                    //todo: error here when game starts out.  is ship spawning in negative sector space?
                                    //todo: if so, a solution could be to run GetDivinedSector() on currentLocation to fix.
                                    //int i;
                                }

                                var sectorToExamine = new Sector(new LocationDef(currentRegion, new Coordinate(sectorT, sectorL)));
                                var locationToExamine = new Location(currentRegion, sectorToExamine);

                                //todo: this could be currentRegion.GetDivinedSector()
                                Location neighborSectorLocation = myLocation.Region.DivineSectorOnMap(locationToExamine, this.Map);

                                if (neighborSectorLocation.Region.Type != RegionType.GalacticBarrier)
                                {
                                    //todo: are we moving the Ship?  what's going on?

                                    //currentResult.Location.Region = neighborSectorLocation.Region;

                                    //sectorsToQuery = currentResult.Location.Region.Sectors;

                                    //this.Map.Write.SingleLine(currentResult.Location.Region.Name);

                                    ////Do we really need this second assignment?
                                    //currentResult.Location.Sector = sectorsToQuery.GetNoError(new Coordinate(lookedUpLocation.Sector.X, lookedUpLocation.Sector.Y, false));
                                }
                            }
                        }
                        row++;
                    }
                }

                row++;
            }
        }

        /// <summary>
        /// This will be called when ship is set down, and after every turn
        /// </summary>
        public void UpdateLocalFiringRange()
        {
            this.Sector.Neighbors = new List<DivinedSectorItem>();
        }
    }
}
