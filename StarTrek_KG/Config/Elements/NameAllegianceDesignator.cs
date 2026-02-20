using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class NameAllegianceDesignator : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name => this["name"] as string;

        [ConfigurationProperty("allegiance", IsRequired = true)]
        public string allegiance => this["allegiance"] as string;

        [ConfigurationProperty("designator", IsRequired = true)]
        public string designator => this["designator"] as string;
    }
}
