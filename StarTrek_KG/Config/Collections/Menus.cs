using System.Configuration;

namespace StarTrek_KG.Config.Collections
{
    public class Menus : NameHeaders
    {
        public new MenuElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as MenuElement;
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

        public new MenuElement this[string responseString]
        {
            get
            {
                return (MenuElement)BaseGet(responseString);
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
            return new MenuElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MenuElement)element).name;
        }
    }
}
