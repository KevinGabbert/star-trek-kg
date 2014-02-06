using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class SeverityValueTranslationElement : ConfigurationElement
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

        [ConfigurationProperty("translation", IsRequired = false)]
        public string translation
        {
            get
            {
                return this["translation"] as string;
            }
        }
    }
}
