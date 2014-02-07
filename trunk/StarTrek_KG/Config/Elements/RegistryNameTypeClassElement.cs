using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class RegistryNameTypeClassElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("registry", IsRequired = false)]
        public string registry
        {
            get
            {
                return this["registry"] as string;
            }
        }

        [ConfigurationProperty("type", IsRequired = false)]
        public string type
        {
            get
            {
                return this["type"] as string;
            }
        }

        [ConfigurationProperty("class", IsRequired = false)]
        public string @class
        {
            get
            {
                return this["class"] as string;
            }
        }
    }
}
