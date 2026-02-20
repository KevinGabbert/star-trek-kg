using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class SubcommandCollection : ConfigurationElementCollection
    {
        public SubcommandElement this[int index]
        {
            get { return (SubcommandElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new SubcommandElement this[string key]
        {
            get { return (SubcommandElement)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SubcommandElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SubcommandElement)element).Key;
        }
    }
}
