using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;

namespace StarTrek_KG.Playfield
{
    public class SectorDefs : List<SectorDef>
    {
        //public IEnumerable<SectorDef> GetHostiles()
        //{
        //    //return this.Where(s => s.Item == SectorItem.Hostile); //TODO: this needs to be changed to a count of ships in a sector that have hostile intent, rather than this "placemarker" construct

        //    var defs = new List<SectorDef>();

        //    //todo: not even sure if this will work.  can we cast an object to a ship?
        //    foreach (SectorDef def in this)
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

        public IEnumerable<SectorDef> Friendlies()
        {
            return this.Where(s => s.Item == SectorItem.FriendlyShip); //TODO: this needs to be changed to a count of ships in a sector that have friendly intent, rather than this "placemarker" construct
        }

        public IEnumerable<SectorDef> ConfigFriendlies()
        {
            return this.Where(s => s.Item == SectorItem.FriendlyShip); //todo: what is this for?
        }
    }
}
