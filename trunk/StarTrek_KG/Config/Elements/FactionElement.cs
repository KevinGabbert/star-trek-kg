using System.Configuration;

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
