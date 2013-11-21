﻿using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;

namespace StarTrek_KG.Playfield
{
    /// <summary>
    /// A Sector in this game is an empty XY position in a quadrant.  Only 1 item can occupy a sector at a time.
    /// </summary>
    public class Sector: Coordinate //todo: this should be called SectorItems (or quadrantItems)
    {
        #region Properties
            public SectorItem Item { get; set; }
            public Coordinate QuadrantDef { get; set; }
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

        public static Sector CreateEmpty(Quadrant quadrant, Coordinate sectorDef)
        {
            var sector = new Sector(new LocationDef(quadrant, sectorDef));
            sector.Item = SectorItem.Empty;

            return sector;
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.  Sets down a friendly 
        /// </summary>
        /// <param name="map"></param>
        public static void SetFriendly(Map map)
        {
            //zip through all sectors in all quadrants.  remove any friendlies

            //This is a bit of a brute force approach, and not preferred, as it disguises any bugs that might have to do with forgetting
            //to remove the ship at the right time.  This function will need to go away or stop being used when or if this game is modified
            //to have multiple friendlies, as is the eventual plan.

            Sector.RemoveAllFriendlies(map);
            Sector.Get(map.Quadrants.GetActive().Sectors, map.Playership.Sector.X,
                                                          map.Playership.Sector.Y).Item = SectorItem.Friendly;
        }

        /// <summary>
        /// Removes all friendlies fromevery sector in the entire map.
        /// </summary>
        /// <param name="map"></param>
        public static void RemoveAllFriendlies(Map map)
        {
            var sectorsWithFriendlies = map.Quadrants.SelectMany(quadrant => quadrant.Sectors.Where(sector => sector.Item == SectorItem.Friendly));

            foreach (Sector sector in sectorsWithFriendlies)
            {
                sector.Item = SectorItem.Empty;
            }
        }
    }
}