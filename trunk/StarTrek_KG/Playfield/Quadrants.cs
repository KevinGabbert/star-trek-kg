using System;
using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Subsystem;

namespace StarTrek_KG.Playfield
{
    public class Quadrants: List<Quadrant>
    {
        #region Fields

            private Hostiles hostiles;

        #endregion

        #region Properties

            public Map Map { get; set; }

        /// <summary>
        /// Movement.Execute and Map.SetupPlayership are the only places this is set
        /// </summary>

        public Hostiles Hostiles
        {
            get { return hostiles; }
        }

        #endregion

        public Quadrants(Map map)
        {
            this.Map = map;
        }

        //todo: query quadrants for number of hostiles (change quadrants to a collection!) and get rid of "totalHostiles variable)

        public static Quadrant Get(Map map, int quadrantX, int quadrantY)
        {
            var i = map.Quadrants.Where(q => q.X == quadrantX && q.Y == quadrantY);
            return i.Single();
        }

        public static Quadrant Get(List<Quadrant> quadrants, int quadrantX, int quadrantY)
        {
            Quadrant retVal = null;

            try
            {
                //todo: change this back to retval
                retVal = quadrants.Where(q => q.X == quadrantX && q.Y == quadrantY).Single();
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

            if(activeQuadrants.Count() < 1)
            {
                throw new GameConfigException("No Quadrant has been set Active - This would happen if there are no friendlies on the map.");
            }

            //There can only be one active sector
            return activeQuadrants.Single();
        }

        public static bool NoHostiles(List<Ship> hostiles)
        {
            if (hostiles.Count == 0)
            {
                Output.Write("There are no Hostile ships in this quadrant.");
                return true;
            }
            return false;
        }

        public int GetHostileCount()
        {
            int allHostiles = this.Sum(q => q.Hostiles.Count);
            return allHostiles;
        }

        public void Remove(List<Ship> shipsToRemove, Map map)
        {
            foreach (var ship in shipsToRemove)
            {
                this.Remove(ship, map);
            }
        }

        public void Remove(Ship shipToRemove, Map map)
        {
            //linq through quadrants and sectors to remove all ships that have the same details as Ship
            Coordinate shipToRemoveQuadrant = shipToRemove.QuadrantDef;

            //todo: output name of destroyed ship:  "IKS Kardasz destroyed"

            if(shipToRemoveQuadrant == null)
            {
                //For now, ships having no location is a config error, that is, until such a time comes that
                //we determine that is actually a feature. 
                //We shouldnt be seeing this kind of error so late in the game.
                //todo: lock this behavior down with a test
                throw new GameConfigException("ship has no location. ");
            }

            //Delete from Map
            map.Get(shipToRemove.QuadrantDef.X, 
                    shipToRemove.QuadrantDef.Y, 
                    shipToRemove.Sector.X, 
                    shipToRemove.Sector.Y).Item = SectorItem.Empty;

            var quadrantToRemoveFrom = map.Quadrants.Single(q => q.X == shipToRemove.QuadrantDef.X &&
                                                                 q.Y == shipToRemove.QuadrantDef.Y);
            //todo: NO HOSTILE HERE TO REMOVE??
            quadrantToRemoveFrom.Hostiles.Remove(quadrantToRemoveFrom.Hostiles.Where(h => h.Sector.X == shipToRemove.Sector.X && 
                                                                                          h.Sector.Y == shipToRemove.Sector.Y &&
                                                                                          h.Name == shipToRemove.Name ).Single()); //Remove from local set of Hostiles.
        }

        /// <summary>
        /// This is actually placeholder code, as hopefully, one day, there will be a lot of playerships running around, needing removal,
        /// but thats not needed at the moment, so if the playership is ever directly removed, then get out and end game.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="shipToRemoveQuadrant"></param>
        private static void CheckForPlayershipRemoval(Map map, Coordinate shipToRemoveQuadrant)
        {
            var playerShipQuadrant = map.Playership.QuadrantDef;

            if (playerShipQuadrant.X == shipToRemoveQuadrant.X &&
                playerShipQuadrant.Y == shipToRemoveQuadrant.Y)
            {
                //Game Over.  Todo: finish this.
                throw new GameException("Game Over. Playership dead. Catch this, output to console, and start over.");
            }
        }

        /// <summary>
        /// TODO: this needs to be changed.  after destruction, it appears to take several method returns to realize that we are dead.
        /// </summary>
        /// <returns></returns>
        public bool ALLHostilesAttack(Map map)
        {
            //todo:rewrite this.
            //this is called from torpedo control/phaser control, and navigation control

            var activeQuadrant = map.Quadrants.GetActive();
            var hostilesAttacking = activeQuadrant.Hostiles;

            //temporary
            if (hostilesAttacking != null)//todo: remove this.
            {
                if (hostilesAttacking.Count > 0)
                {
                    foreach (var badGuy in hostilesAttacking)
                    {
                        if (Navigation.For(map.Playership).docked)
                        {
                            Console.WriteLine(
                                map.Playership.Name +
                                " hit by " + badGuy.Name + " at sector [{0},{1}]. No damage due to starbase shields.",
                                (badGuy.Sector.X), (badGuy.Sector.Y));
                        }
                        else
                        {
                            if (!Ship.AbsorbHitFrom(badGuy, map)) return true;
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        //todo: refactor this?
        public bool NotFound(int x, int y)
        {
            var notFound = this.Where(s => s.X == x && s.Y == y).Count() == 0;
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
        private static IEnumerable<Sector> SectorsWithShips(Quadrant quadrant)
        {
            return quadrant.Sectors.Where(sector => sector.Item == SectorItem.Friendly || sector.Item == SectorItem.Hostile);
        }

    }
}
