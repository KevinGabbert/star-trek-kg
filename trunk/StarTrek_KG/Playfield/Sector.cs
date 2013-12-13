using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Sector in this game is an empty XY position in a quadrant.  Only 1 item can occupy a sector at a time.
    /// </summary>
    public class Sector: Coordinate //todo: this should be called SectorItems (or quadrantItems)
    {

        #region Properties

            //TODO: get rid of enum. 
            //TODO: A SectorItem needs to be the *actual* object held.  (a star, or ship (I want a starbase to be a type of ship so it can have hitpoints, shield, be hostile, and fight back.))
            public SectorItem Item { get; set; }
            public ISectorObject Object { get; set; }

            public Coordinate QuadrantDef { get; set; } //needed.  so it can set ship coordinate

        #endregion


        //todo: create a constructor overload that will let you create a sector without an associated quadrant

        public Sector(LocationDef location)
        {
            this.X = location.Sector.X;
            this.Y = location.Sector.Y;
            this.QuadrantDef = location.Quadrant;
        }

        public Sector(SectorDef sectorDef)
        {
            this.X = sectorDef.Sector.X;
            this.Y = sectorDef.Sector.Y;
            this.QuadrantDef = sectorDef.QuadrantDef;
        }

        public static Sector Get(Sectors sectors, int sX, int sY)
        {
            var gotSectors = sectors.Where(s => s.X == sX && s.Y == sY).ToList();

            if (gotSectors.Count() < 1)
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
            var shipQuadrant = shipToGetFrom.GetQuadrant();
            var gotSectors = shipQuadrant.Sectors.Where(s => s.X == shipToGetFrom.Sector.X && s.Y == shipToGetFrom.Sector.Y).ToList();

            if (gotSectors.Count() < 1)
            {
                throw new GameConfigException("Sector not found:  X: " + shipToGetFrom.Sector.X + " Y: " + shipToGetFrom.Sector.Y + " Total Sectors: " + shipQuadrant.Sectors.Count());
            }

            if (gotSectors.Count() > 1)
            {
                throw new GameConfigException("Multiple sectors found. X: " + shipToGetFrom.Sector.X + " Y: " + shipToGetFrom.Sector.Y + " Total Sectors: " + shipQuadrant.Sectors.Count());
            }

            //There can only be one active sector
            return gotSectors.Single();
        }

        public static Sector CreateEmpty(Quadrant quadrant, Coordinate sectorDef)
        {
            var sector = new Sector(new LocationDef(quadrant, sectorDef));
            sector.Item = SectorItem.Empty;

            return sector;
        }

        public override string ToString()
        {
            return "Sector: " + this.X + ", " + this.Y;
        }
    }
}
