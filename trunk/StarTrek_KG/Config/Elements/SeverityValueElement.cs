using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class SeverityValueElement : ConfigurationElement
    {
        [ConfigurationProperty("severity", IsRequired = true)]
        public string severity
        {
            get
            {
                return this["severity"] as string;
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
