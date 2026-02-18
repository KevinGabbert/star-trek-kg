using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class SubcommandElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("prompt", IsRequired = false)]
        public string Prompt
        {
            get { return (string)this["prompt"]; }
            set { this["prompt"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)this["description"]; }
            set { this["description"] = value; }
        }
    }
}
