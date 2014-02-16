using System.Configuration;
using StarTrek_KG.Config.Collections;

namespace StarTrek_KG.Config.Elements
{
    public class Faction : NameAllegianceDesignator
    {
        [ConfigurationProperty("FactionThreats", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FactionThreats), AddItemName = "FactionThreat")]
        public FactionThreats FactionThreats
        {
            get
            {
                return (FactionThreats)base["FactionThreats"];
            }
        }

        [ConfigurationProperty("FactionShips", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FactionShips), AddItemName = "FactionShip")]
        public FactionShips FactionShips
        {
            get
            {
                return (FactionShips)base["FactionShips"];
            }
        }
    }
}
