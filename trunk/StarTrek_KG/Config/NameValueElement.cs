using System.Configuration;

namespace StarTrek_KG.Config
{
    public class NameValueElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string value
        {
            get
            {
                return this["value"] as string;
            }
        }
    }
}
