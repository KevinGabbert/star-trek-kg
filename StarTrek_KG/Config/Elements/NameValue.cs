using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class NameValue : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name => this["name"] as string;

        [ConfigurationProperty("value", IsRequired = true)]
        public string value => this["value"] as string;
    }
}
