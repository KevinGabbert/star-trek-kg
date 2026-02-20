using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;

namespace StarTrek_KG.Playfield
{
    public class CoordinateDefs : List<CoordinateDef>
    {
        //public IEnumerable<CoordinateDef> GetHostiles()
        //{
        //    //return this.Where(s => s.Item == CoordinateItem.Hostile); //TODO: this needs to be changed to a count of ships in a sector that have hostile intent, rather than this "placemarker" construct

        //    var defs = new List<CoordinateDef>();

        //    //todo: not even sure if this will work.  can we cast an object to a ship?
        //    foreach (CoordinateDef def in this)
        //    {
        //        if(def.Object != null)
        //        {
        //            var currentShip = (IShip) def.Object;

        //            if (currentShip != null)
        //            {
        //                if (currentShip.Allegiance == Allegiance.BadGuy)
        //                {
        //                    defs.Add(def);
        //                }
        //            }
        //        }
        //    }

        //    return defs; 
        //}

        public IEnumerable<CoordinateDef> PlayerShips()
        {
            return this.Where(s => s.Item == CoordinateItem.PlayerShip); //TODO: this needs to be changed to a count of ships in a sector that have friendly intent, rather than this "placemarker" construct
        }

        //public IEnumerable<CoordinateDef> ConfigFriendlies()
        //{
        //    return this.Where(s => s.Item == CoordinateItem.PlayerShip); //todo: what is this for?
        //}
    }
}
