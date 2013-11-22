using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;

namespace StarTrek_KG.Playfield
{
    public class SectorDefs : List<SectorDef>
    {
        public IEnumerable<SectorDef> Hostiles()
        {
            return this.Where(s => s.Item == SectorItem.Hostile); //TODO: this needs to be changed to a count of ships in a sector that have hostile intent, rather than this "placemarker" construct
        }

        public IEnumerable<SectorDef> Friendlies()
        {
            return this.Where(s => s.Item == SectorItem.Friendly); //TODO: this needs to be changed to a count of ships in a sector that have friendly intent, rather than this "placemarker" construct
        }

        public IEnumerable<SectorDef> ConfigFriendlies()
        {
            return this.Where(s => s.Item == SectorItem.Friendly); //todo: what is this for?
        }
    }
}
