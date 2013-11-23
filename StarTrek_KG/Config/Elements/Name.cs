using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class Name : ConfigurationElement
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
