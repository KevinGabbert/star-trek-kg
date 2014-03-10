﻿using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Playfield
{
    public class Regions: List<Region>
    {
        #region Properties

            public IMap Map { get; set; }
            public IOutputWrite Write { get; set; }

            /// <summary>
            /// Movement.Execute and Map.SetupPlayership are the only places this is set
            /// </summary>

        #endregion

        public Regions(IMap map, IOutputWrite write)
        {
            this.Write = write;
            this.Map = map;
        }

        public static Region Get(IMap map, Coordinate Region)
        {
            var i = map.Regions.Where(q => q.X == Region.X && q.Y == Region.Y);
            return i.Single();
        }

        public static Region Get(IEnumerable<Region> Regions, Coordinate Region)
        {
            Region retVal = null;

            try
            {
                //todo: change this back to retval
                retVal = Regions.Single(q => q.X == Region.X && q.Y == Region.Y);
            }
            catch(InvalidOperationException ex)
            {
                if(ex.Message == "Sequence contains more than one element")
                {
                    //todo: why do we have more than 1 element???
                }
            }
 
            return retVal;
        }

        public static Region GetByName(IEnumerable<Region> Regions, string RegionName)
        {
            Region retVal = null;

            try
            {
                //todo: change this back to retval
                retVal = Regions.Single(q => q.Name == RegionName);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Sequence contains more than one element.  The fix for this is to remove dupes from app.config")
                {
                    //todo: why do we have more than 1 element?
                }
            }

            return retVal;
        }

        public Region GetActive()
        {
            var activeRegions = this.Map.Regions.Where(q => q.Active).ToList();

            if(!activeRegions.Any())
            {
                throw new GameConfigException("No Region has been set Active - This would happen if there are no friendlies on the map.");
            }

            //There can only be one active sector
            return activeRegions.Single();
        }

        public bool NoHostiles(List<IShip> hostiles)
        {
            if (hostiles.Count == 0)
            {
                (new Write(this.Map.Config)).Line("There are no Hostile ships in this Region.");
                return true;
            }
            return false;
        }

        public List<IShip> GetHostiles()
        {
            var allHostiles = new List<IShip>();

            foreach (Region Region in this)
            {
                allHostiles.AddRange(Region.GetHostiles());
            }

            return allHostiles;
        }

        public int GetHostileCount()
        {
            int allHostiles = this.Sum(q => q.GetHostiles().Count);
            return allHostiles;
        }

        public void Remove(IEnumerable<IShip> shipsToRemove)
        {
            foreach (var ship in shipsToRemove)
            {
                this.Remove(ship);
            }
        }

        /// <summary>
        /// goes through each sector in this Region and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void RemoveShip(string name)
        {
            Sector sectorToDeleteShip = null;

            //There should only be 1 ship with this name

            foreach (var Region in this)
            {
                if (Region.Sectors != null)
                {
                    foreach (var sector in Region.Sectors)
                    {
                        var @object = sector.Object;

                        if (@object != null)
                        {
                            if (@object.Type.Name == "Ship")
                            {
                                var possibleShipToDelete = (IShip) @object;
                                if (possibleShipToDelete.Name == name)
                                {
                                    sectorToDeleteShip = Regions.GetFoundShip(name, sector, sectorToDeleteShip);
                                    goto DeleteNow;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new GameException("No Sectors Set up in Region: " + Region.Name + "...");
                }
            }

            DeleteNow:
            if (sectorToDeleteShip == null)
            {
                throw new GameException("Unexpected State. " + name + " not found.");
            }

            sectorToDeleteShip.Item = SectorItem.Empty;
            sectorToDeleteShip.Object = null;
        }

        private static Sector GetFoundShip(string name, Sector sector, Sector sectorToDeleteShip)
        {
            if (sectorToDeleteShip == null)
            {
                sectorToDeleteShip = sector;
            }
            else
            {
                throw new GameException("Unexpected State. > instance of " + name + " found.");
            }

            return sectorToDeleteShip;
        }

        public void Remove(IShip shipToRemove)
        {
            //linq through Regions and sectors to remove all ships that have the same details as Ship
            Coordinate shipToRemoveRegion = shipToRemove.Coordinate;

            if(shipToRemoveRegion == null)
            {
                //For now, ships having no location is a config error, that is, until such a time comes that
                //we determine that is actually a feature. 
                //We shouldnt be seeing this kind of error so late in the game.
                //todo: lock this behavior down with a test
                throw new GameConfigException("ship has no location. ");
            }

            this.RemoveShip(shipToRemove.Name);

            string shipToRemoveName = shipToRemove.Name;

            if (shipToRemove.Faction == FactionName.Federation)
            {
                shipToRemoveName = shipToRemove.Name;
                this.Map.AddACoupleHostileFederationShipsToExistingMap();
            }

            this.Write.Line(string.Format("{2} {3} [{0},{1}].", (shipToRemove.Sector.X), (shipToRemove.Sector.Y), shipToRemoveName, this.Map.Config.GetText("shipDestroyed")));
        }

        public bool NotFound(Coordinate coordinate)
        {
            var notFound = this.Count(s => s.X == coordinate.X && s.Y == coordinate.Y) == 0;
            return notFound;
        }

        public void ClearActive()
        {
            var RegionsToClear = this.Where(q => q.Active).ToList();

            foreach (var Region in RegionsToClear)
            {
                Region.Active = false;
            }

            if(RegionsToClear.Count() > 1)
            {
                //log a config error
            }
        }

        public int GetStarbaseCount()
        {
            int allStarbases = this.Sum(q => q.GetStarbaseCount());
            return allStarbases;
        }

        /// <summary>
        /// yes. it really sux that you can't warp through a nebula.  We don't know whats in there.  LRS fails.. We won't know how to keep things on course.
        /// but.. its only a temporary problem.  You can warp *out* of a nebula, so its really meant to be an annoying obstacle which 
        /// could actually affect some situations (time, for example)</summary>
        /// <param name="map"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        internal static bool IsNebula(IMap map, Coordinate coordinate)
        {
            //todo: make an app.config setting to turn this feature off if people whine.
            return (Regions.Get(map, coordinate).Type == RegionType.Nebulae);
        }
    }
}

    ///// <summary>
    ///// This is actually placeholder code, as hopefully, one day, there will be a lot of playerships running around, needing removal,
    ///// but thats not needed at the moment, so if the playership is ever directly removed, then get out and end game.
    ///// </summary>
    ///// <param name="map"></param>
    ///// <param name="shipToRemoveRegion"></param>
    //private static void CheckForPlayershipRemoval(IMap map, Coordinate shipToRemoveRegion)
    //{
    //    var playerShipRegion = map.Playership.Coordinate;

    //    if (playerShipRegion.X == shipToRemoveRegion.X &&
    //        playerShipRegion.Y == shipToRemoveRegion.Y)
    //    {
    //        //Game Over.  Todo: finish this.
    //        throw new GameException("Game Over. Playership dead. Catch this, output to console, and start over.");
    //    }
    //}


    //public bool NotFound(int x, int y)
    //{
    //    var notFound = this.Count(s => s.X == x && s.Y == y) == 0;
    //    return notFound;
    //}

    //private static IEnumerable<Sector> SectorsWithShips(Region Region)
    //{
    //    return Region.Sectors.Where(sector => sector.Item == SectorItem.Friendly || sector.Item == SectorItem.Hostile);
    //}

//public void RemoveFromAnywhere()
//{
//    //foreach (Region Region in map.Regions.Where(q => q.X == shipToRemove.RegionDef.X && q.Y == shipToRemove.RegionDef.Y))
//    //{
//    //    Region Region1 = Region;
//    //    foreach (Sector sector in SectorsWithShips(Region).Where(sector => Region1.X == shipToRemoveRegion.X &&
//    //                                                                         Region1.Y == shipToRemoveRegion.Y &&
//    //                                                                         sector.X == shipToRemove.Sector.X &&
//    //                                                                         sector.Y == shipToRemove.Sector.Y))
//    //    {
//    //        sector.Item = SectorItem.Empty; //remove its placemarker from the map

//    //        Region.Hostiles.Remove(shipToRemove);//Remove from Region.Hostiles.  //remove 1 hostile from this Region count
//    //        map.Regions.Hostiles.Remove(shipToRemove); //remove from our big list of baddies //Remove from Regions.Hostiles. //todo: is this needed?

//    //        //Remove from this.Map.Playership
//    //        Regions.CheckForPlayershipRemoval(map, shipToRemoveRegion);
//    //    }
//    //}
//}
