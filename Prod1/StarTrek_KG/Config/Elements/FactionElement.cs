using System.Configuration;
using StarTrek_KG.Config.Collections;

namespace StarTrek_KG.Config.Elements
{
    public class FactionElement : NameAllegianceElement
    {
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
