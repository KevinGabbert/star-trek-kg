﻿using System;
using System.Text;
using StarTrek_KG.Enums;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Playfield;
using StarTrek_KG.Subsystem;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Output
{
    public class PrintSector: IWrite, IConfig
    {
        private Location Location { get; set; }
        public string Condition { get; set; }
        private int ShieldsDownLevel { get; set; }
        private int LowEnergyLevel { get; set; }

        public IOutputWrite Write { get; set; }
        public IStarTrekKGSettings Config { get; set; }

        public PrintSector(int shieldsDownLevel, int lowEnergyLevel, IOutputWrite write, IStarTrekKGSettings config)
        {
            this.Config = config;
            this.Write = write;
            this.Write.Config = config;

            this.ShieldsDownLevel = shieldsDownLevel;
            this.LowEnergyLevel = lowEnergyLevel;
        }

        public void SRSPrintSector(Quadrant quadrant, IMap map)
        {
            var condition = this.GetCurrentCondition(quadrant, map);

            Location myLocation = map.Playership.GetLocation();
            int totalHostiles = map.Quadrants.GetHostileCount();
            bool docked = Navigation.For(map.Playership).Docked;

            bool shieldsAutoRaised = false;
            if (quadrant.GetHostiles().Count > 0)
            {
                shieldsAutoRaised = Game.Auto_Raise_Shields(map, quadrant);
            }

            this.CreateViewScreen(quadrant, map, totalHostiles, condition, myLocation, docked);
            this.OutputSRSWarnings(quadrant, map, docked);

            if (shieldsAutoRaised)
            {
                map.Write.Line("Shields automatically raised to " + Shields.For(map.Playership).Energy);
            }
        }

        private void CreateViewScreen(IQuadrant quadrant,
                                      IMap map,
                                      int totalHostiles,
                                      string condition,
                                      Location location,
                                      bool docked)
        {
            var sb = new StringBuilder();
            this.Condition = condition;
            this.Location = location;

            this.Write.Console.WriteLine("");

            var quadrantName = quadrant.Name;
            var isNebula = (quadrant.Type == QuadrantType.Nebulae);

            if (isNebula)
            {
                quadrantName += " Nebula"; //todo: resource out.
            }

            this.Write.Console.WriteLine(this.Config.GetText("SRSTopBorder", "SRSRegionIndicator"), quadrantName);

            for (int i = 0; i < 8; i++ ) //todo: resource out
            {
                this.ShowSectorRow(sb, i, this.GetRowIndicator(i, map), quadrant.Sectors, totalHostiles, isNebula);
            }

            this.Write.Console.WriteLine(this.Config.GetText("SRSBottomBorder", "SRSDockedIndicator"), docked);
        }

        private string GetRowIndicator(int row, IMap map)
        {
            string retVal = " ";

            switch (row)
            {
                case 0:
                    retVal += String.Format(this.Config.GetText("SRSQuadrantIndicator"), Convert.ToString(this.Location.Quadrant.X), Convert.ToString(this.Location.Quadrant.Y));
                    break;
                case 1:
                    retVal += String.Format(this.Config.GetText("SRSSectorIndicator"), Convert.ToString(this.Location.Sector.X), Convert.ToString(this.Location.Sector.Y));
                    break;
                case 2:
                    retVal += String.Format(this.Config.GetText("SRSStardateIndicator"), map.Stardate);
                    break;
                case 3:
                    retVal += String.Format(this.Config.GetText("SRSTimeRemainingIndicator"), map.timeRemaining);
                    break;
                case 4:
                    retVal += String.Format(this.Config.GetText("SRSConditionIndicator"), this.Condition);
                    break;
                case 5:
                    retVal += String.Format(this.Config.GetText("SRSEnergyIndicator"), map.Playership.Energy);
                    break;
                case 6:
                    retVal += String.Format(this.Config.GetText("SRSShieldsIndicator"), Shields.For(map.Playership).Energy);
                    break;
                case 7:
                    retVal += String.Format(this.Config.GetText("SRSTorpedoesIndicator"), Torpedoes.For(map.Playership).Count);
                    break;
            }

            return retVal;
        }

        private void ShowSectorRow(StringBuilder sb, int row, string suffix, Sectors sectors, int totalHostiles, bool isNebula)
        {
            for (var column = 0; column < Constants.SECTOR_MAX; column++)
            {
                Sector sector = Sector.Get(sectors, row, column);

                switch (sector.Item)
                {
                    case SectorItem.Empty:

                        sb.Append(isNebula ? Utility.Utility.NebulaUnit() : Constants.EMPTY);
                        break;

                    case SectorItem.Friendly:
                        sb.Append(Constants.PLAYERSHIP);
                        break;

                    case SectorItem.Hostile:

                        this.AppendFactionDesignator(sb, totalHostiles, sector);
                        break;

                    case SectorItem.Star:
                        sb.Append(Constants.STAR);
                        break;

                    case SectorItem.Starbase:
                        //todo:  this.AppendFactionDesignator(sb, totalHostiles, sector);
                        //this code will be used when starbase is an object

                        sb.Append(Constants.STARBASE);
                        break;

                    case SectorItem.Debug:
                        sb.Append(Constants.DEBUG_MARKER);
                        break;

                    default:
                        sb.Append(Constants.NULL_MARKER);
                        break;
                }
            }
            if (suffix != null)
            {
                sb.Append(suffix);
            }

            this.Write.Console.WriteLine(sb.ToString());
            sb.Length = 0;
        }

        private void AppendFactionDesignator(StringBuilder sb, int totalHostiles, Sector sector)
        {
            var ship = (IShip) sector.Object;

            //todo: get designator for faction

            string factionDesignator = this.Config.Get.FactionDetails(ship.Faction).designator;

            if (factionDesignator == "")
            {
                factionDesignator = "+?+";
            }

            //bug can be viewed (and even tested here)
            //if last hostile was destroyed, it wont be removed from array.

            //noticed in playthrough that last hostile couldn't be seen.  is it properly labeled?

            //todo: resolve if issue
            //if (this.game.Map.Hostiles.Count < 1)
            //{
            //    Console.WriteLine("bug. hostile not removed from display.");
            //}

            if (totalHostiles < 1)
            {
                this.Write.Console.WriteLine("bug. hostile not removed from display.");
            }

            //todo: hostile feds look like: ++-
            //if (sector.Object.Faction == Faction.Federation)
            //{
            //    sb.Append(Constants.FEDERATION);
            //}

            sb.Append(factionDesignator);
        }

        private string GetCurrentCondition(IQuadrant quadrant, IMap map)
        {
            var condition = "GREEN";

            if (quadrant.GetHostiles().Count > 0)
            {
                condition = "RED";
            }
            else if (map.Playership.Energy < this.LowEnergyLevel)
            {
                condition = "YELLOW";
            }

            return condition;
        }

        private void OutputSRSWarnings(Quadrant quadrant, IMap map, bool docked)
        {
            if (quadrant.GetHostiles().Count > 0)
            {                
                this.SRSScanHostile(quadrant);
            }
            else if (map.Playership.Energy < this.LowEnergyLevel) //todo: setting comes from app.config
            {
                this.Write.ResourceLine("LowEnergyLevel");
            }

            if (quadrant.Type == QuadrantType.Nebulae)
            {
                this.Write.SingleLine("");
                this.Write.ResourceLine("NebulaWarning");
            }

            if (Shields.For(map.Playership).Energy == this.ShieldsDownLevel && !docked)
            {
                this.Write.ResourceLine("ShieldsDown");
            }
        }

        //todo: this function needs to be part of SRS
        private void SRSScanHostile(Quadrant quadrant)
        {
            this.Write.Console.WriteLine(this.Config.GetText("HostileDetected"),
                              (quadrant.GetHostiles().Count == 1 ? "" : "s"));

            bool inNebula = quadrant.Type == QuadrantType.Nebulae;

            foreach (var hostile in quadrant.GetHostiles())
            {
                var hostileName = hostile.Name;

                if (inNebula)
                {
                    hostileName = "Unknown";
                }

                if (hostile.Faction == FactionName.Federation)
                {
                    hostileName = hostile.Name + " " + Game.GetFederationShipRegistration(hostile);
                }

                this.Write.Console.WriteLine(this.Config.GetText("IDHostile"), hostileName);
            }

            this.Write.Console.WriteLine("");
        }
    }
}
