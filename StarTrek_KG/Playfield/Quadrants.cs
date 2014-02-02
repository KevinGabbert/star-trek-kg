using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.Output;

namespace StarTrek_KG.Playfield
{
    public class Quadrants: List<Quadrant>
    {
        #region Properties

            public IMap Map { get; set; }
            public IOutputWrite Write { get; set; }

            /// <summary>
            /// Movement.Execute and Map.SetupPlayership are the only places this is set
            /// </summary>

        #endregion

        public Quadrants(IMap map, IOutputWrite write)
        {
            this.Write = write;
            this.Map = map;
        }

        //todo: query quadrants for number of hostiles (change quadrants to a collection!) and get rid of "totalHostiles variable)

        public static Quadrant Get(IMap map, Coordinate quadrant)
        {
            var i = map.Quadrants.Where(q => q.X == quadrant.X && q.Y == quadrant.Y);
            return i.Single();
        }

        public static Quadrant Get(IEnumerable<Quadrant> quadrants, Coordinate quadrant)
        {
            Quadrant retVal = null;

            try
            {
                //todo: change this back to retval
                retVal = quadrants.Single(q => q.X == quadrant.X && q.Y == quadrant.Y);
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

        public Quadrant GetActive()
        {
            var activeQuadrants = this.Map.Quadrants.Where(q => q.Active).ToList();

            if(!activeQuadrants.Any())
            {
                throw new GameConfigException("No Quadrant has been set Active - This would happen if there are no friendlies on the map.");
            }

            //There can only be one active sector
            return activeQuadrants.Single();
        }

        public bool NoHostiles(List<IShip> hostiles)
        {
            if (hostiles.Count == 0)
            {
                (new Write(this.Map.Config)).Line("There are no Hostile ships in this quadrant.");
                return true;
            }
            return false;
        }

        public List<IShip> GetHostiles()
        {
            var allHostiles = new List<IShip>();

            foreach (Quadrant quadrant in this)
            {
                allHostiles.AddRange(quadrant.GetHostiles());
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
        /// goes through each sector in this quadrant and clears hostiles
        /// </summary>
        /// <returns></returns>
        public void RemoveShip(string name)
        {
            Sector sectorToDeleteShip = null;

            //There should only be 1 ship with this name

            foreach (var quadrant in this)
            {
                if (quadrant.Sectors != null)
                {
                    foreach (var sector in quadrant.Sectors)
                    {
                        var @object = sector.Object;

                        if (@object != null)
                        {
                            if (@object.Type.Name == "Ship")
                            {
                                var possibleShipToDelete = (IShip) @object;
                                if (possibleShipToDelete.Name == name)
                                {
                                    sectorToDeleteShip = Quadrants.GetFoundShip(name, sector, sectorToDeleteShip);
                                    goto DeleteNow;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new GameException("No Sectors Set up in Quadrant: " + quadrant.Name + "...");
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
            //linq through quadrants and sectors to remove all ships that have the same details as Ship
            Coordinate shipToRemoveQuadrant = shipToRemove.Coordinate;

            if(shipToRemoveQuadrant == null)
            {
                //For now, ships having no location is a config error, that is, until such a time comes that
                //we determine that is actually a feature. 
                //We shouldnt be seeing this kind of error so late in the game.
                //todo: lock this behavior down with a test
                throw new GameConfigException("ship has no location. ");
            }

            this.RemoveShip(shipToRemove.Name);

            this.Write.Line(string.Format("{2} {3} [{0},{1}].", (shipToRemove.Sector.X), (shipToRemove.Sector.Y), shipToRemove.Name, this.Map.Config.GetText("shipDestroyed")));
        }

        /// <summary>
        /// This is actually placeholder code, as hopefully, one day, there will be a lot of playerships running around, needing removal,
        /// but thats not needed at the moment, so if the playership is ever directly removed, then get out and end game.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="shipToRemoveQuadrant"></param>
        private static void CheckForPlayershipRemoval(Map map, Coordinate shipToRemoveQuadrant)
        {
            var playerShipQuadrant = map.Playership.Coordinate;

            if (playerShipQuadrant.X == shipToRemoveQuadrant.X &&
                playerShipQuadrant.Y == shipToRemoveQuadrant.Y)
            {
                //Game Over.  Todo: finish this.
                throw new GameException("Game Over. Playership dead. Catch this, output to console, and start over.");
            }
        }

        //public bool NotFound(int x, int y)
        //{
        //    var notFound = this.Count(s => s.X == x && s.Y == y) == 0;
        //    return notFound;
        //}

        public bool NotFound(Coordinate coordinate)
        {
            var notFound = this.Count(s => s.X == coordinate.X && s.Y == coordinate.Y) == 0;
            return notFound;
        }

        public void ClearActive()
        {
            var quadrantsToClear = this.Where(q => q.Active).ToList();

            foreach (var quadrant in quadrantsToClear)
            {
                quadrant.Active = false;
            }

            if(quadrantsToClear.Count() > 1)
            {
                //log a config error
            }
        }

        public void RemoveFromAnywhere()
        {
            //foreach (Quadrant quadrant in map.Quadrants.Where(q => q.X == shipToRemove.QuadrantDef.X && q.Y == shipToRemove.QuadrantDef.Y))
            //{
            //    Quadrant quadrant1 = quadrant;
            //    foreach (Sector sector in SectorsWithShips(quadrant).Where(sector => quadrant1.X == shipToRemoveQuadrant.X &&
            //                                                                         quadrant1.Y == shipToRemoveQuadrant.Y &&
            //                                                                         sector.X == shipToRemove.Sector.X &&
            //                                                                         sector.Y == shipToRemove.Sector.Y))
            //    {
            //        sector.Item = SectorItem.Empty; //remove its placemarker from the map

            //        quadrant.Hostiles.Remove(shipToRemove);//Remove from Quadrant.Hostiles.  //remove 1 hostile from this quadrant count
            //        map.Quadrants.Hostiles.Remove(shipToRemove); //remove from our big list of baddies //Remove from Quadrants.Hostiles. //todo: is this needed?

            //        //Remove from this.Map.Playership
            //        Quadrants.CheckForPlayershipRemoval(map, shipToRemoveQuadrant);
            //    }
            //}
        }

        //private static IEnumerable<Sector> SectorsWithShips(Quadrant quadrant)
        //{
        //    return quadrant.Sectors.Where(sector => sector.Item == SectorItem.Friendly || sector.Item == SectorItem.Hostile);
        //}
    }
}
