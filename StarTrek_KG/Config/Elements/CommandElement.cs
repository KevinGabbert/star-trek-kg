using System.Configuration;
using StarTrek_KG.Config.Collections;

namespace StarTrek_KG.Config.Elements
{
    public class CommandElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("subsystem", IsRequired = false)]
        public string Subsystem
        {
            get { return (string)this["subsystem"]; }
            set { this["subsystem"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)this["description"]; }
            set { this["description"] = value; }
        }

        [ConfigurationProperty("Subcommands")]
        [ConfigurationCollection(typeof(SubcommandCollection), AddItemName = "Subcommand")]
        public SubcommandCollection Subcommands
        {
            get { return (SubcommandCollection)this["Subcommands"]; }
        }
    }
}
