using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class NameAllegianceDesignator : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("allegiance", IsRequired = true)]
        public string allegiance
        {
            get
            {
                return this["allegiance"] as string;
            }
        }

        [ConfigurationProperty("designator", IsRequired = true)]
        public string designator
        {
            get
            {
                return this["designator"] as string;
            }
        }
    }
}
