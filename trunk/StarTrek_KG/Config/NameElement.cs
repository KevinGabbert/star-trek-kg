using System.Configuration;

namespace StarTrek_KG.Config
{
    public class NameElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get
            {
                return this["name"] as string;
            }
        }
    }
}
