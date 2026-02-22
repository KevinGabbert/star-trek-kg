using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Settings;
using StarTrek_KG.Types;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Coordinate in this game is an empty XY position in a Sector.  Only 1 item can occupy a sector at a time.
    /// </summary>
    public class Coordinate : Point, ICoordinate //todo: this should be called SectorItems (or SectorItems)
    {
        #region Properties

            //TODO: get rid of enum. 
            //TODO: A CoordinateItem needs to be the *actual* object held.  (a star, or ship (I want a starbase to be a type of ship so it can have hitpoints, shield, be hostile, and fight back.))
            public CoordinateItem Item { get; set; }
            public ICoordinateObject Object { get; set; }
            public CoordinateType Type { get; set; }
            public Point SectorDef { get; set; } //needed.  so it can set ship coordinate
            public List<DivinedCoordinateItem> Neighbors { get; set; }
            public bool Scanned { get; set; }

        #endregion

        //todo: create a constructor overload that will let you create a sector without an associated Sector

        public Coordinate()
        {
            this.Type = CoordinateType.Space;
        }

        public Coordinate(LocationDef location)
        {
            this.Type = CoordinateType.Space;
            this.X = location.Coordinate.X;
            this.Y = location.Coordinate.Y;
            this.SectorDef = location.Sector;
        }

        public static Coordinate Get(Coordinates sectors, int sX, int sY)
        {
            var gotSectors = sectors.Where(s => s.X == sX && s.Y == sY).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException($"Coordinate not found:  X: {sX} Y: {sY} Total Coordinates: {sectors.Count}");
            }

            if (gotSectors.Count > 1)
            {
                throw new GameConfigException($"Multiple sectors found. X: {sX} Y: {sY} Total Coordinates: {sectors.Count}");
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public static Coordinate GetFrom(IShip shipToGetFrom)
        {
            Sector shipSector = shipToGetFrom.GetSector();
            List<Coordinate> gotSectors = shipSector.Coordinates.Where(s => s.X == shipToGetFrom.Coordinate.X && s.Y == shipToGetFrom.Coordinate.Y).ToList();

            if (!gotSectors.Any())
            {
                throw new GameConfigException("Coordinate not found:  X: " + shipToGetFrom.Coordinate.X + " Y: " + shipToGetFrom.Coordinate.Y + " Total Coordinates: " + shipSector.Coordinates.Count);
            }

            if (gotSectors.Count > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + shipToGetFrom.Coordinate.X + " Y: " + shipToGetFrom.Coordinate.Y + " Total Coordinates: " + shipSector.Coordinates.Count);
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public static Coordinate CreateEmpty(Sector Sector, Point coordinatePoint)
        {
            var sector = new Coordinate(new LocationDef(Sector, coordinatePoint))
            {
                Item = CoordinateItem.Empty
            };

            return sector;
        }

        public override string ToString()
        {
            return $"Coordinate: {this.X}, {this.Y}";
        }

        public static int SIncrement(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension >= DEFAULTS.COORDINATE_MAX)
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

        public static int SDecrement(int coordinateDimension)
        {
            int retVal;

            if (coordinateDimension < DEFAULTS.COORDINATE_MIN)
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

        internal static string GetNeighborDirection(Point currentSector, Point sectorCoordinateToGet)
        {
            throw new NotImplementedException();
        }

        internal static IRSResult GetInfo(LocationDef locationToScan)
        {
            throw new NotImplementedException();
        }

        internal static IRSResult GetInfo(IMap iMap, LocationDef locationToScan)
        {
            //copy LRS Render

            //var tlrsResults = shipLocation.Sector.GetLRSFullData(shipLocation, this.Game);
            //var renderedData = this.Game.Write.RenderLRSData(tlrsResults, this.Game);
            throw new NotImplementedException();
        }

        public Point GetPoint()
        {
            return new Point(this.X, this.Y);
        }

        public bool Invalid()
        {
            bool invalid = this.X < DEFAULTS.COORDINATE_MIN || 
                           this.X >= DEFAULTS.COORDINATE_MAX ||
                           this.Y < DEFAULTS.COORDINATE_MIN ||
                           this.Y >= DEFAULTS.COORDINATE_MAX;

            return invalid;
        }

        public void IncrementForNewSector()
        {
            int currentX = this.X;
            int currentY = this.Y;

            int resetValue = DEFAULTS.COORDINATE_MAX + 1;

            if (this.X < DEFAULTS.COORDINATE_MIN)
            {
                this.X = currentX - resetValue;
            }

            if (this.Y < DEFAULTS.COORDINATE_MIN)
            {
                this.Y = currentY - resetValue;
            }

            if (this.X > DEFAULTS.COORDINATE_MAX)
            {
                this.X = resetValue - currentX;
            }

            if (this.Y >= DEFAULTS.COORDINATE_MAX)
            {
                this.Y = resetValue - currentY;
            }
        }
    }
}

    //public static Coordinate FromCoordinate(Point coordinateToConvert)
    //{
    //    var newSector = new Coordinate();
    //    newSector.X = coordinateToConvert.X;
    //    newSector.Y = coordinateToConvert.Y;

    //    return newSector;
    //}
    //public Coordinate(CoordinateDef sectorDef)
    //{
    //    this.Type = CoordinateType.Space;
    //    this.X = sectorDef.Coordinate.X;
    //    this.Y = sectorDef.Coordinate.Y;
    //    this.SectorDef = sectorDef.SectorDef;
    //}
