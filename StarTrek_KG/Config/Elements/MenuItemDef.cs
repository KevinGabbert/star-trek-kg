using System.Configuration;

namespace StarTrek_KG.Config.Elements
{
    public class MenuItemDef : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name => this["name"] as string;

        [ConfigurationProperty("promptLevel", IsRequired = true)]
        public int? promptLevel => this["promptLevel"] as int?;

        [ConfigurationProperty("description", IsRequired = true)]
        public string description => this["description"] as string;

        [ConfigurationProperty("ordinalPosition", IsRequired = true)]
        public int? ordinalPosition => this["ordinalPosition"] as int?;

        [ConfigurationProperty("divider", IsRequired = true)]
        public string divider => this["divider"] as string;
    }
}
