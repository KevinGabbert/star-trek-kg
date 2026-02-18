using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class MenuElementCollection : ConfigurationElementCollection
    {
        public new MenuElement this[int index]
        {
            get { return (MenuElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new MenuElement this[string name]
        {
            get { return (MenuElement)BaseGet(name); }
            set
            {
                if (BaseGet(name) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(name)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MenuElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MenuElement)element).name;
        }
    }
}
