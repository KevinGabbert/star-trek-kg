using System.Configuration;

namespace StarTrek_KG.Config
{
    public class StarSystem : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }
    }
}
