using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class MenuElement : NameHeader
    {
        [ConfigurationProperty("MenuItems")]
        [ConfigurationCollection(typeof(MenuItems), AddItemName = "MenuItem")]
        public MenuItems MenuItems => (MenuItems)base["MenuItems"];
    }
}
