using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class KGNameAllegianceCollection: ConfigurationElementCollection
    {
        public NameAllegianceElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameAllegianceElement;
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

        public new NameAllegianceElement this[string responseString]
        {
            get
            {
                return (NameAllegianceElement)BaseGet(responseString);
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
            return new NameAllegianceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameAllegianceElement)element).name;
        }
    }
}
