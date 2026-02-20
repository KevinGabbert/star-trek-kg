using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using StarTrek_KG.Config.Collections;
using StarTrek_KG.Commands;

namespace StarTrek_KG.Config.Elements
{
    public class MenusElement : ConfigurationElement
    {
        [ConfigurationProperty("Commands")]
        [ConfigurationCollection(typeof(CommandCollection), AddItemName = "Command")]
        public CommandCollection Commands
        {
            get { return (CommandCollection)this["Commands"]; }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(MenuElementCollection), AddItemName = "MenuElement")]
        public MenuElementCollection MenuElements
        {
            get { return (MenuElementCollection)this[""]; }
        }
    }
}

