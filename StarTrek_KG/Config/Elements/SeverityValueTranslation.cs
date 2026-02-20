using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class SeverityValueTranslation : ConfigurationElement
    {
        [ConfigurationProperty("severity", IsRequired = true)]
        public string severity => this["severity"] as string;

        [ConfigurationProperty("value", IsRequired = true)]
        public string value => this["value"] as string;

        [ConfigurationProperty("translation", IsRequired = false)]
        public string translation => this["translation"] as string;
    }
}
