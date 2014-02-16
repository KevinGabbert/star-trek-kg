using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class NameAllegianceDesignators: ConfigurationElementCollection
    {
        public NameAllegianceDesignator this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameAllegianceDesignator;
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

        public new NameAllegianceDesignator this[string responseString]
        {
            get
            {
                return (NameAllegianceDesignator)BaseGet(responseString);
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
            return new NameAllegianceDesignator();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameAllegianceDesignator)element).name;
        }
    }
}
