using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class NameHeader : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name => this["name"] as string;

        [ConfigurationProperty("header", IsRequired = true)]
        public string header => this["header"] as string;
    }
}
