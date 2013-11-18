using System.Collections.Generic;
using System.Linq;
using StarTrek_KG.Enums;

namespace StarTrek_KG.Playfield
{
    public class SectorDefs : List<SectorDef>
    {
        public IEnumerable<SectorDef> Hostiles()
        {
            return this.Where(s => s.Item == SectorItem.Hostile);
        }

        public IEnumerable<SectorDef> Friendlies()
        {
            return this.Where(s => s.Item == SectorItem.Friendly);
        }

        public IEnumerable<SectorDef> ConfigFriendlies()
        {
            return this.Where(s => s.Item == SectorItem.Friendly);
        }
    }
}
