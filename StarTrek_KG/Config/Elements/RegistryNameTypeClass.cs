using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class RegistryNameTypeClass : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name => this["name"] as string;

        [ConfigurationProperty("registry", IsRequired = false)]
        public string registry => this["registry"] as string;

        [ConfigurationProperty("type", IsRequired = false)]
        public string type => this["type"] as string;

        [ConfigurationProperty("class", IsRequired = false)]
        public string @class => this["class"] as string;
    }
}
