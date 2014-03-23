using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Sector in this game is an empty XY position in a Region.  Only 1 item can occupy a sector at a time.
    /// </summary>
    public class Sector: Coordinate, ISector //todo: this should be called SectorItems (or RegionItems)
    {
        #region Properties

            //TODO: get rid of enum. 
            //TODO: A SectorItem needs to be the *actual* object held.  (a star, or ship (I want a starbase to be a type of ship so it can have hitpoints, shield, be hostile, and fight back.))
            public SectorItem Item { get; set; }
            public ISectorObject Object { get; set; }
            public SectorType Type { get; set; }
            public Coordinate RegionDef { get; set; } //needed.  so it can set ship coordinate
            public List<SectorNeighborItem> Neighbors { get; set; }

        #endregion

        //todo: create a constructor overload that will let you create a sector without an associated Region

        public Sector()
        {
            this.Type = SectorType.Space;
        }

        public Sector(LocationDef location)
        {
            this.Type = SectorType.Space;
            this.X = location.Sector.X;
            this.Y = location.Sector.Y;
            this.RegionDef = location.Region;
        }

        public static Sector Get(Sectors sectors, int sX, int sY)
        {
            var gotSectors = sectors.Where(s => s.X == sX && s.Y == sY).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException("Sector not found:  X: " + sX + " Y: " + sY + " Total Sectors: " + sectors.Count());
            }

            if (gotSectors.Count() > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + sX + " Y: " + sY + " Total Sectors: " + sectors.Count());
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public static Sector GetFrom(Ship shipToGetFrom)
        {
            var shipRegion = shipToGetFrom.GetRegion();
            var gotSectors = shipRegion.Sectors.Where(s => s.X == shipToGetFrom.Sector.X && s.Y == shipToGetFrom.Sector.Y).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException("Sector not found:  X: " + shipToGetFrom.Sector.X + " Y: " + shipToGetFrom.Sector.Y + " Total Sectors: " + shipRegion.Sectors.Count());
            }

            if (gotSectors.Count() > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + shipToGetFrom.Sector.X + " Y: " + shipToGetFrom.Sector.Y + " Total Sectors: " + shipRegion.Sectors.Count());
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public static Sector CreateEmpty(Region Region, Coordinate sectorDef)
        {
            var sector = new Sector(new LocationDef(Region, sectorDef));
            sector.Item = SectorItem.Empty;

            return sector;
        }

        public override string ToString()
        {
            return "Sector: " + this.X + ", " + this.Y;
        }

        public static int Increment(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension >= Constants.SECTOR_MAX)
            {
                retVal = 0;
            }
            else
            {
                //todo: write a test for this in particular.  
                if (coordinateDimension < 7)
                {
                    retVal = coordinateDimension + 1;
                }
                else
                {
                    retVal = coordinateDimension;
                }
            }

            return retVal;
        }
        public static int Decrement(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension < Constants.SECTOR_MIN)
            {
                retVal = 7;
            }

            else
            {
                //todo: write a test for this in particular. 
                if (coordinateDimension > 0)
                {
                    retVal = coordinateDimension - 1;
                }
                else
                {
                    retVal = coordinateDimension;
                }
            }

            return retVal;
        }
    }
}

    //public static Sector FromCoordinate(Coordinate coordinateToConvert)
    //{
    //    var newSector = new Sector();
    //    newSector.X = coordinateToConvert.X;
    //    newSector.Y = coordinateToConvert.Y;

    //    return newSector;
    //}
    //public Sector(SectorDef sectorDef)
    //{
    //    this.Type = SectorType.Space;
    //    this.X = sectorDef.Sector.X;
    //    this.Y = sectorDef.Sector.Y;
    //    this.RegionDef = sectorDef.RegionDef;
    //}