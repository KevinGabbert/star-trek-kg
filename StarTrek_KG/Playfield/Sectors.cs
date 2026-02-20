using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Constants;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Extensions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Playfield
{
    public class Sectors: List<Sector>
    {
        public Sector this[int x, int y] => this.Get(new Point(x, y));
        public Sector this[Point region] => this.Get(region);
        public Sector this[string regionName] => this.Get(regionName);

        #region Properties

        private IMap Map { get; set; }
            private IInteraction Write { get; set; }

            /// <summary>
            /// Movement.Execute and Map.SetupPlayership are the only places this is set
            /// </summary>

        #endregion

        public Sectors(IMap map, IInteraction write)
        {
            this.Write = write;
            this.Map = map;
        }

        /// <summary>
        /// basically, this just means "out of bounds" of the passed map
        /// </summary>
        /// <param name="regionX"></param>
        /// <param name="regionY"></param>
        /// <param name="regionToGet"></param>
        /// <returns></returns>
        public bool IsGalacticBarrier(Point regionToGet)
        {
           Sector gotRegion = this.Map?.Sectors?.FirstOrDefault(region => region.X == regionToGet.X && region.Y == regionToGet.Y);

            return gotRegion == null;
        }

        ////todo: this should go away
        //public static Sector Get(IMap map, Point region)
        //{
        //    IEnumerable<Sector> i = map.Sectors.Where(q => q.X == region.X && q.Y == region.Y);

        //    return i.DefaultIfEmpty(new Sector(map, region.X, region.Y, SectorType.GalacticBarrier)).Single();
        //}

        public Sector Get(Point region)
        {
            IEnumerable<Sector> i = this.Map.Sectors.Where(q => q.X == region.X && q.Y == region.Y);

            return i.DefaultIfEmpty(new Sector(this.Map, region.X, region.Y, SectorType.GalacticBarrier)).Single();
        }

        public Sector Get(string name)
        {
            var i = this.Where(q => q.Name == name);
            return i.Single();
        }

        public static Sector Get(IEnumerable<Sector> Sectors, Point Sector)
        {
            Sector retVal = null;

            try
            {
                //todo: change this back to retval
                retVal = Sectors.Single(q => q.X == Sector.X && q.Y == Sector.Y);
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

        //public Sector GetName(Map map, string regionName)
        //{
        //    Sector regionToScan = map.Sectors.Get(regionName);

        //    var outOfBounds = false;

        //    if (!outOfBounds)
        //    {
        //        currentResult = this.GetRegionData(regionX, regionY, game);
        //    }
        //    else
        //    {
        //        currentResult.GalacticBarrier = true;
        //    }

        //    var barrierID = "Galactic Barrier"; //todo: resource this
        //    regionResult.Name = regionResult.GalacticBarrier ? barrierID : regionToScan.Name;

        //    return regionResult;
        //}

        public Sector GetActive()
        {
            var activeRegions = this.Map.Sectors.Where(q => q.Active).ToList();

            if(!activeRegions.Any())
            {
                throw new GameConfigException("No Sector has been set Active - This would happen if there are no friendlies on the map.");
            }

            //There can only be one active sector
            return activeRegions.Single();
        }

        public bool NoHostiles(IEnumerable<IShip> hostiles, out List<string> outputLines)
        {
            outputLines = new List<string>();

            if (!hostiles.Any())
            {
                outputLines = new Interaction(this.Map.Config).Line("There are no Hostile ships in this Sector."); //todo: resource this
                return true;
            }
            return false;
        }

        public List<IShip> GetHostiles()
        {
            var allHostiles = new List<IShip>();

            foreach (Sector Sector in this)
            {
                allHostiles.AddRange(Sector.GetHostiles());
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
            foreach (IShip ship in shipsToRemove)
            {
                this.Remove(ship);
            }
        }

        /// <summary>
        /// goes through each sector in this Sector and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void RemoveShip(IShip ship)
        {
            Coordinate sectorToDeleteShip = null;

            Sector regionToDeleteFrom = ship.GetSector();

            sectorToDeleteShip = (from sector in regionToDeleteFrom.Coordinates

                                    let sectorObject = sector.Object
                                    where sectorObject != null
                                    where sectorObject.Type.Name == OBJECT_TYPE.SHIP

                                    let possibleShipToDelete = (IShip) sectorObject
                                    where possibleShipToDelete.Name == ship.Name
                                    select sector).SingleOrDefault();  //There should only be 1 ship with this name          

            if (sectorToDeleteShip == null)
            {
                throw new GameException($"Unexpected State. {ship.Name} not found.");
            }

            sectorToDeleteShip.Item = CoordinateItem.Empty;
            sectorToDeleteShip.Object = null;
        }

        /// <summary>
        /// Gets the next region in the direction provided
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="map"></param>
        /// <param name="currentRegion"></param>
        /// <returns></returns>
        internal static Sector GetNext(IMap map, Sector currentRegion, NavDirection direction)
        {
            int newRegionX = 0;
            int newRegionY = 0;

            switch (direction)
            {
                case NavDirection.Right:
                    newRegionX = currentRegion.X + 1;
                    break;
                
                case NavDirection.RightDown:
                    newRegionX = currentRegion.X + 1;
                    newRegionY = currentRegion.Y + 1;
                    break;

                case NavDirection.Down:
                    newRegionY = currentRegion.Y + 1;
                    break;

                case NavDirection.LeftDown:
                    newRegionX = currentRegion.X - 1;
                    newRegionY = currentRegion.Y + 1;
                    break;

                case NavDirection.Left:
                    newRegionX = currentRegion.X - 1;
                    break;

                case NavDirection.LeftUp:
                    newRegionX = currentRegion.X - 1;
                    break;

                case NavDirection.Up:
                    newRegionY = currentRegion.Y - 1;
                    break;

                case NavDirection.RightUp:
                    newRegionY = currentRegion.Y - 1;
                    newRegionX = currentRegion.X + 1;
                    break;

                default:
                    throw new ArgumentException("Not a valid Sector");
            }

            Sector newRegion = new Sector(new Point(newRegionX, newRegionY));

            var returnVal = !newRegion.Invalid() ? map.Sectors[newRegion] : null;
            return returnVal;
        }

        /// <summary>
        /// goes through each sector in this Sector and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void RemoveShipFromMap(string shipName)
        {
            try
            {
                Coordinate sectorWithShipToDelete = this.Select(region => SectorsWithMatchingShips(shipName, region)
                                                    .SingleOrDefault()
                                                    ).SingleOrDefault(s => s != null);

                if (sectorWithShipToDelete != null)
                {
                    sectorWithShipToDelete.Item = CoordinateItem.Empty;
                    sectorWithShipToDelete.Object = null;
                }
                else
                {
                    throw new GameException($"Unexpected State. {shipName} not found.");
                }
            }
            catch
            {
                throw new GameException($"Unexpected State. {shipName} not found.");
            }
        }

        private static IEnumerable<Coordinate> SectorsWithMatchingShips(string shipName, ISector region)
        {
            IEnumerable<Coordinate> sectorsWithMatchingShips =  from sector in region.Coordinates

                                                            let sectorObject = sector.Object

                                                            where sectorObject != null
                                                            where sectorObject.Type.Name == OBJECT_TYPE.SHIP

                                                            let possibleShipToDelete = (IShip) sectorObject

                                                            where possibleShipToDelete.Name == shipName
                                                            select sector;
            return sectorsWithMatchingShips;
        }

        public void Remove(IShip shipToRemove)
        {
            //linq through Sectors and sectors to remove all ships that have the same details as Ship
            Point shipToRemoveRegion = shipToRemove.Point;

            if(shipToRemoveRegion == null)
            {
                //For now, ships having no location is a config error, that is, until such a time comes that
                //we determine that is actually a feature. 
                //We shouldnt be seeing this kind of error so late in the game.
                //todo: lock this behavior down with a test
                throw new GameConfigException("ship has no location. ");
            }

            this.RemoveShip(shipToRemove);

            string shipToRemoveName = shipToRemove.Name;

            if (shipToRemove.Faction == FactionName.Federation)
            {
                shipToRemoveName = shipToRemove.Name;
                this.Map.AddACoupleHostileFederationShipsToExistingMap(); //todo: this needs to be configurable
            }

            this.Write.Line(string.Format("{2} {3} [{0},{1}].", shipToRemove.Coordinate.X, shipToRemove.Coordinate.Y, shipToRemoveName, this.Map.Config.GetText("shipDestroyed")));
        }

        public bool NotFound(Point coordinate)
        {
            return !this.Any(s => s.X == coordinate.X && s.Y == coordinate.Y);
        }

        public void ClearActive()
        {
            if (!this.IsNullOrEmpty())
            {
                foreach (var region in this)
                {
                    region.Active = false;
                }
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
        internal static bool IsNebula(IMap map, Point coordinate)
        {
            //todo: make an app.config setting to turn this feature off if people whine.
            return map.Sectors[coordinate].Type == SectorType.Nebulae;
        }
    }
}

    ///// <summary>
    ///// This is actually placeholder code, as hopefully, one day, there will be a lot of playerships running around, needing removal,
    ///// but thats not needed at the moment, so if the playership is ever directly removed, then get out and end game.
    ///// </summary>
    ///// <param name="map"></param>
    ///// <param name="shipToRemoveRegion"></param>
    //private static void CheckForPlayershipRemoval(IMap map, Point shipToRemoveRegion)
    //{
    //    var playerShipRegion = map.Playership.Point;

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

    //private static IEnumerable<Coordinate> SectorsWithShips(Sector Sector)
    //{
    //    return Sector.Coordinates.Where(sector => sector.Item == CoordinateItem.Friendly || sector.Item == CoordinateItem.Hostile);
    //}

//public void RemoveFromAnywhere()
//{
//    //foreach (Sector Sector in map.Sectors.Where(q => q.X == shipToRemove.SectorDef.X && q.Y == shipToRemove.SectorDef.Y))
//    //{
//    //    Sector Region1 = Sector;
//    //    foreach (Coordinate sector in SectorsWithShips(Sector).Where(sector => Region1.X == shipToRemoveRegion.X &&
//    //                                                                         Region1.Y == shipToRemoveRegion.Y &&
//    //                                                                         sector.X == shipToRemove.Coordinate.X &&
//    //                                                                         sector.Y == shipToRemove.Coordinate.Y))
//    //    {
//    //        sector.Item = CoordinateItem.Empty; //remove its placemarker from the map

//    //        Sector.Hostiles.Remove(shipToRemove);//Remove from Sector.Hostiles.  //remove 1 hostile from this Sector count
//    //        map.Sectors.Hostiles.Remove(shipToRemove); //remove from our big list of baddies //Remove from Sectors.Hostiles. //todo: is this needed?

//    //        //Remove from this.Map.Playership
//    //        Sectors.CheckForPlayershipRemoval(map, shipToRemoveRegion);
//    //    }
//    //}
//}
