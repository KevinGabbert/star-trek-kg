using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class MenuItemDefs : ConfigurationElementCollection
    {
        public MenuItemDef this[int index]
        {
            get
            {
                return base.BaseGet(index) as MenuItemDef;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new MenuItemDef this[string responseString]
        {
            get
            {
                return (MenuItemDef)BaseGet(responseString);
            }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }

                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MenuItemDef();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MenuItemDef)element).name;
        }
    }
}
